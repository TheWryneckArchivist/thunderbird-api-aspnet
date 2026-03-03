namespace Thunderbird.Api.Infrastructure.Persistence.Entities;

public sealed class AuthSession
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string RefreshTokenHash { get; set; } = string.Empty;
  public string DeviceFingerprint { get; set; } = string.Empty;
  public long CreatedAtUnix { get; set; }
  public long ExpiresAtUnix { get; set; }
  public long? RevokedAtUnix { get; set; }
  public string? LastIdempotencyKey { get; set; }
  public string? LastAccessToken { get; set; }
  public long? LastAccessTokenExpiresAtUnix { get; set; }

  public UserAccount User { get; set; } = null!;
}
