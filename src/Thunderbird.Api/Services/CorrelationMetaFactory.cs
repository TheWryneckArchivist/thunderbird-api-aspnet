using Grpc.Core;
using Thunderbird.Contracts.V1;

namespace Thunderbird.Api.Services;

internal static class CorrelationMetaFactory
{
  public static CorrelationMeta Create(ServerCallContext context)
  {
    return new CorrelationMeta
    {
      RequestId = Guid.NewGuid().ToString("N"),
      CorrelationId = context.RequestHeaders.GetValue("x-correlation-id") ?? Guid.NewGuid().ToString("N"),
      CausationId = context.RequestHeaders.GetValue("x-causation-id") ?? string.Empty,
      OccurredAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };
  }
}
