namespace Thunderbird.Api.Infrastructure.Persistence.Entities;

public sealed class DiscordOAuthState
{
  public Guid Id { get; set; }
  public string State { get; set; } = string.Empty;
  public string RedirectUri { get; set; } = string.Empty;
  public int Platform { get; set; }
  public long CreatedAtUnix { get; set; }
  public long ExpiresAtUnix { get; set; }
  public long? ConsumedAtUnix { get; set; }
}
