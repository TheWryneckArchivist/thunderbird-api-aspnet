using Grpc.Core;
using Thunderbird.Contracts.V1;
using Thunderbird.GameRules.Offline;

namespace Thunderbird.Api.Services;

public sealed class OfflineSyncGrpcService : OfflineSyncService.OfflineSyncServiceBase
{
  public override Task<UploadOfflineDeltaResponse> UploadOfflineDelta(UploadOfflineDeltaRequest request, ServerCallContext context)
  {
    return Task.FromResult(new UploadOfflineDeltaResponse
    {
      DeltaId = Guid.NewGuid().ToString("N"),
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<ValidateOfflineDeltaResponse> ValidateOfflineDelta(ValidateOfflineDeltaRequest request, ServerCallContext context)
  {
    var response = new ValidateOfflineDeltaResponse
    {
      MergeReportId = Guid.NewGuid().ToString("N"),
      Meta = CorrelationMetaFactory.Create(context)
    };

    response.Results.Add(new ActionValidationResult
    {
      ActionId = "sample-action",
      Accepted = true,
      ReasonCode = OfflineReasonCodes.Accepted,
      DeltaClass = DeltaClass.Cosmetic
    });

    return Task.FromResult(response);
  }

  public override Task<CommitSelectiveMergeResponse> CommitSelectiveMerge(CommitSelectiveMergeRequest request, ServerCallContext context)
  {
    return Task.FromResult(new CommitSelectiveMergeResponse
    {
      AcceptedCount = 1,
      RejectedCount = 0,
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<GetMergeReportResponse> GetMergeReport(GetMergeReportRequest request, ServerCallContext context)
  {
    var response = new GetMergeReportResponse
    {
      Meta = CorrelationMetaFactory.Create(context)
    };

    response.Results.Add(new ActionValidationResult
    {
      ActionId = "sample-action",
      Accepted = true,
      ReasonCode = OfflineReasonCodes.Accepted,
      DeltaClass = DeltaClass.Cosmetic
    });

    return Task.FromResult(response);
  }
}
