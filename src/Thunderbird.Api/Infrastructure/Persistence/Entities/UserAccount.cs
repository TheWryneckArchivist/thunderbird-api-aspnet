namespace Thunderbird.Api.Infrastructure.Persistence.Entities;

public sealed class UserAccount
{
  public Guid Id { get; set; }
  public string DisplayName { get; set; } = string.Empty;
  public long CreatedAtUnix { get; set; }
  public long UpdatedAtUnix { get; set; }

  public ICollection<AuthIdentity> Identities { get; set; } = new List<AuthIdentity>();
  public ICollection<AuthSession> Sessions { get; set; } = new List<AuthSession>();
}
