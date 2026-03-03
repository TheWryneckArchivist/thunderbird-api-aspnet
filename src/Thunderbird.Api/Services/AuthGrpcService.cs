using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Thunderbird.Api.Infrastructure.Persistence;
using Thunderbird.Api.Infrastructure.Persistence.Entities;
using Thunderbird.Api.Infrastructure.Security;
using Thunderbird.Contracts.V1;

namespace Thunderbird.Api.Services;

public sealed class AuthGrpcService : AuthService.AuthServiceBase
{
  private const string DiscordProvider = "discord";
  private readonly PlatformDbContext _dbContext;
  private readonly AuthOptions _authOptions;

  public AuthGrpcService(PlatformDbContext dbContext, IOptions<AuthOptions> authOptions)
  {
    _dbContext = dbContext;
    _authOptions = authOptions.Value;
  }

  public override async Task<BeginDiscordOAuthResponse> BeginDiscordOAuth(BeginDiscordOAuthRequest request, ServerCallContext context)
  {
    if (string.IsNullOrWhiteSpace(request.RedirectUri))
    {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "redirect_uri is required"));
    }

    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var state = Guid.NewGuid().ToString("N");

    _dbContext.DiscordOAuthStates.Add(new DiscordOAuthState
    {
      Id = Guid.NewGuid(),
      State = state,
      RedirectUri = request.RedirectUri.Trim(),
      Platform = (int)request.Platform,
      CreatedAtUnix = now,
      ExpiresAtUnix = now + (_authOptions.OAuthStateLifetimeMinutes * 60)
    });

    await _dbContext.SaveChangesAsync(context.CancellationToken);

    return new BeginDiscordOAuthResponse
    {
      AuthUrl = BuildDiscordAuthorizeUrl(state, request.RedirectUri),
      State = state,
      Meta = CorrelationMetaFactory.Create(context)
    };
  }

  public override async Task<ExchangeDiscordCodeResponse> ExchangeDiscordCode(ExchangeDiscordCodeRequest request, ServerCallContext context)
  {
    if (string.IsNullOrWhiteSpace(request.Code))
    {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "code is required"));
    }

    if (string.IsNullOrWhiteSpace(request.State))
    {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "state is required"));
    }

    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var oauthState = await _dbContext.DiscordOAuthStates
      .FirstOrDefaultAsync(x => x.State == request.State, context.CancellationToken);

    if (oauthState is null || oauthState.ConsumedAtUnix.HasValue || oauthState.ExpiresAtUnix < now)
    {
      throw new RpcException(new Status(StatusCode.FailedPrecondition, "invalid or expired OAuth state"));
    }

    oauthState.ConsumedAtUnix = now;

    var providerUserId = BuildDeterministicDiscordSubject(request.Code);
    var identity = await _dbContext.AuthIdentities
      .Include(x => x.User)
      .FirstOrDefaultAsync(x => x.Provider == DiscordProvider && x.ProviderUserId == providerUserId, context.CancellationToken);

    var displayName = $"DiscordUser-{providerUserId[..8]}";
    UserAccount user;
    if (identity is null)
    {
      user = new UserAccount
      {
        Id = Guid.NewGuid(),
        DisplayName = displayName,
        CreatedAtUnix = now,
        UpdatedAtUnix = now
      };

      identity = new AuthIdentity
      {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        Provider = DiscordProvider,
        ProviderUserId = providerUserId,
        DisplayName = displayName,
        LinkedAtUnix = now,
        UpdatedAtUnix = now
      };

      user.Identities.Add(identity);
      _dbContext.UserAccounts.Add(user);
    }
    else
    {
      user = identity.User;
      user.UpdatedAtUnix = now;
      identity.DisplayName = displayName;
      identity.UpdatedAtUnix = now;
    }

    var accessTokenExpiresAt = now + (_authOptions.AccessTokenLifetimeMinutes * 60);
    var refreshTokenExpiresAt = now + (_authOptions.RefreshTokenLifetimeDays * 24 * 60 * 60);
    var refreshToken = GenerateOpaqueToken();
    var accessToken = CreateAccessToken(user.Id, accessTokenExpiresAt);

    _dbContext.AuthSessions.Add(new AuthSession
    {
      Id = Guid.NewGuid(),
      UserId = user.Id,
      RefreshTokenHash = HashToken(refreshToken),
      DeviceFingerprint = NormalizeDeviceFingerprint(request.DeviceFingerprint),
      CreatedAtUnix = now,
      ExpiresAtUnix = refreshTokenExpiresAt,
      LastAccessToken = accessToken,
      LastAccessTokenExpiresAtUnix = accessTokenExpiresAt
    });

    await _dbContext.SaveChangesAsync(context.CancellationToken);

    return new ExchangeDiscordCodeResponse
    {
      UserId = user.Id.ToString(),
      Tokens = new AuthTokens
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAtUnix = accessTokenExpiresAt
      },
      Meta = CorrelationMetaFactory.Create(context)
    };
  }

  public override async Task<RefreshAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest request, ServerCallContext context)
  {
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "refresh_token is required"));
    }

    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var refreshTokenHash = HashToken(request.RefreshToken);
    var session = await _dbContext.AuthSessions
      .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, context.CancellationToken);

    if (session is null || session.RevokedAtUnix.HasValue || session.ExpiresAtUnix <= now)
    {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "refresh token is invalid or expired"));
    }

    if (!string.IsNullOrWhiteSpace(request.IdempotencyKey)
      && session.LastIdempotencyKey == request.IdempotencyKey
      && !string.IsNullOrWhiteSpace(session.LastAccessToken)
      && session.LastAccessTokenExpiresAtUnix.HasValue)
    {
      return new RefreshAccessTokenResponse
      {
        Tokens = new AuthTokens
        {
          AccessToken = session.LastAccessToken,
          RefreshToken = request.RefreshToken,
          ExpiresAtUnix = session.LastAccessTokenExpiresAtUnix.Value
        },
        Meta = CorrelationMetaFactory.Create(context)
      };
    }

    var accessTokenExpiresAt = now + (_authOptions.AccessTokenLifetimeMinutes * 60);
    var accessToken = CreateAccessToken(session.UserId, accessTokenExpiresAt);

    session.LastAccessToken = accessToken;
    session.LastAccessTokenExpiresAtUnix = accessTokenExpiresAt;
    session.LastIdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
      ? null
      : request.IdempotencyKey.Trim();

    await _dbContext.SaveChangesAsync(context.CancellationToken);

    return new RefreshAccessTokenResponse
    {
      Tokens = new AuthTokens
      {
        AccessToken = accessToken,
        RefreshToken = request.RefreshToken,
        ExpiresAtUnix = accessTokenExpiresAt
      },
      Meta = CorrelationMetaFactory.Create(context)
    };
  }

  public override async Task<GetLinkedIdentitiesResponse> GetLinkedIdentities(GetLinkedIdentitiesRequest request, ServerCallContext context)
  {
    if (!Guid.TryParse(request.UserId, out var userId))
    {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id must be a GUID"));
    }

    var identities = await _dbContext.AuthIdentities
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .OrderBy(x => x.LinkedAtUnix)
      .ToListAsync(context.CancellationToken);

    var response = new GetLinkedIdentitiesResponse
    {
      Meta = CorrelationMetaFactory.Create(context)
    };

    foreach (var identity in identities)
    {
      response.Identities.Add(new LinkedIdentity
      {
        Provider = identity.Provider,
        ProviderUserId = identity.ProviderUserId,
        DisplayName = identity.DisplayName,
        LinkedAtUnix = identity.LinkedAtUnix
      });
    }

    return response;
  }

  private string BuildDiscordAuthorizeUrl(string state, string redirectUri)
  {
    var query = new Dictionary<string, string>
    {
      ["response_type"] = "code",
      ["client_id"] = _authOptions.DiscordClientId,
      ["redirect_uri"] = redirectUri,
      ["scope"] = _authOptions.DiscordScope,
      ["state"] = state,
      ["prompt"] = "none"
    };

    var encoded = string.Join("&", query.Select(pair =>
      $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

    return $"{_authOptions.DiscordAuthorizeUrl}?{encoded}";
  }

  private string CreateAccessToken(Guid userId, long expiresAtUnix)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SigningKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
      issuer: _authOptions.Issuer,
      audience: _authOptions.Audience,
      claims:
      [
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        new Claim("role", "player")
      ],
      notBefore: DateTime.UtcNow,
      expires: DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix).UtcDateTime,
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private static string GenerateOpaqueToken()
  {
    Span<byte> bytes = stackalloc byte[32];
    RandomNumberGenerator.Fill(bytes);
    return Convert.ToBase64String(bytes);
  }

  private static string HashToken(string token)
  {
    var input = Encoding.UTF8.GetBytes(token);
    var hash = SHA256.HashData(input);
    return Convert.ToHexString(hash);
  }

  private static string BuildDeterministicDiscordSubject(string code)
  {
    return HashToken(code.Trim().ToLowerInvariant())[..32].ToLowerInvariant();
  }

  private static string NormalizeDeviceFingerprint(string fingerprint)
  {
    return string.IsNullOrWhiteSpace(fingerprint)
      ? "unknown"
      : fingerprint.Trim()[..Math.Min(fingerprint.Trim().Length, 128)];
  }
}
