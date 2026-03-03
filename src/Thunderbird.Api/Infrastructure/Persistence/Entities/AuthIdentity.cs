namespace Thunderbird.Api.Infrastructure.Persistence.Entities;

public sealed class AuthIdentity
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string Provider { get; set; } = string.Empty;
  public string ProviderUserId { get; set; } = string.Empty;
  public string DisplayName { get; set; } = string.Empty;
  public long LinkedAtUnix { get; set; }
  public long UpdatedAtUnix { get; set; }

  public UserAccount User { get; set; } = null!;
}
