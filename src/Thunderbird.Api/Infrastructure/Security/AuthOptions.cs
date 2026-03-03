namespace Thunderbird.Api.Infrastructure.Security;

public sealed class AuthOptions
{
  public string Issuer { get; set; } = "thunderbird.local";
  public string Audience { get; set; } = "thunderbird-clients";
  public string SigningKey { get; set; } = "dev-signing-key-change-me";
  public string DiscordClientId { get; set; } = "discord-client-id";
  public string DiscordAuthorizeUrl { get; set; } = "https://discord.com/oauth2/authorize";
  public string DiscordScope { get; set; } = "identify";
  public int AccessTokenLifetimeMinutes { get; set; } = 60;
  public int RefreshTokenLifetimeDays { get; set; } = 30;
  public int OAuthStateLifetimeMinutes { get; set; } = 10;
}
