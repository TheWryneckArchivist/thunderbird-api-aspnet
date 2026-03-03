using Grpc.Core;
using Thunderbird.Contracts.V1;

namespace Thunderbird.Api.Services;

public sealed class CatalogGrpcService : CatalogService.CatalogServiceBase
{
  public override Task<ListPacksResponse> ListPacks(ListPacksRequest request, ServerCallContext context)
  {
    var response = new ListPacksResponse
    {
      Meta = CorrelationMetaFactory.Create(context),
      Page = new PageResponse { HasMore = false, NextCursor = string.Empty }
    };

    response.Packs.Add(new PackSummary
    {
      PackId = "pack-001",
      Title = "Bionix Core",
      Category = "Tabletop",
      Tag = "latest",
      OwnerUserId = "user-placeholder"
    });

    return Task.FromResult(response);
  }

  public override Task<GetPackDetailResponse> GetPackDetail(GetPackDetailRequest request, ServerCallContext context)
  {
    var detail = new PackDetail
    {
      Summary = new PackSummary
      {
        PackId = request.PackId,
        Title = "Pack Detail",
        Category = "Tabletop",
        Tag = "stable",
        OwnerUserId = "user-placeholder"
      },
      Description = "Contract-driven placeholder detail.",
      UpdatedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };
    detail.AssetUrls.Add("https://example.invalid/assets/placeholder.png");

    return Task.FromResult(new GetPackDetailResponse
    {
      Pack = detail,
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<SubmitPackDraftResponse> SubmitPackDraft(SubmitPackDraftRequest request, ServerCallContext context)
  {
    return Task.FromResult(new SubmitPackDraftResponse
    {
      PackId = Guid.NewGuid().ToString("N"),
      Status = "draft_received",
      Meta = CorrelationMetaFactory.Create(context)
    });
  }
}
