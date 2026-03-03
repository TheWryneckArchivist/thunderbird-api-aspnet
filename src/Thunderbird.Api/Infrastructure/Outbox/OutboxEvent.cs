namespace Thunderbird.Api.Infrastructure.Outbox;

public sealed class OutboxEvent
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public string EventType { get; set; } = string.Empty;
  public string PayloadJson { get; set; } = string.Empty;
  public long CreatedAtUnix { get; set; }
  public bool IsPublished { get; set; }
}
