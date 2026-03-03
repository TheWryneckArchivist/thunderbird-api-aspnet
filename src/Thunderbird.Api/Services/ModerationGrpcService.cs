using Grpc.Core;
using Thunderbird.Contracts.V1;

namespace Thunderbird.Api.Services;

public sealed class ModerationGrpcService : ModerationService.ModerationServiceBase
{
  public override Task<ReportContentResponse> ReportContent(ReportContentRequest request, ServerCallContext context)
  {
    return Task.FromResult(new ReportContentResponse
    {
      ReportId = Guid.NewGuid().ToString("N"),
      Status = "queued",
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<GetModerationStatusResponse> GetModerationStatus(GetModerationStatusRequest request, ServerCallContext context)
  {
    return Task.FromResult(new GetModerationStatusResponse
    {
      ReportId = request.ReportId,
      Status = "under_review",
      ResolutionCode = string.Empty,
      Meta = CorrelationMetaFactory.Create(context)
    });
  }
}
