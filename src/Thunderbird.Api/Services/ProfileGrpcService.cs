using Grpc.Core;
using Thunderbird.Contracts.V1;

namespace Thunderbird.Api.Services;

public sealed class ProfileGrpcService : ProfileService.ProfileServiceBase
{
  public override Task<GetProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
  {
    var profile = new PlayerProfile
    {
      UserId = request.UserId,
      DisplayName = "Archivist",
      Locale = "en",
      Experience = 0,
      Level = 1
    };
    profile.Roles.Add(ActorRole.Player);

    return Task.FromResult(new GetProfileResponse
    {
      Profile = profile,
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
  {
    var profile = new PlayerProfile
    {
      UserId = request.UserId,
      DisplayName = request.DisplayName,
      Locale = request.Locale,
      Experience = 0,
      Level = 1
    };
    profile.Roles.Add(ActorRole.Player);

    return Task.FromResult(new UpdateProfileResponse
    {
      Profile = profile,
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<GetEntitlementsResponse> GetEntitlements(GetEntitlementsRequest request, ServerCallContext context)
  {
    var response = new GetEntitlementsResponse
    {
      Meta = CorrelationMetaFactory.Create(context)
    };

    response.Entitlements.Add(new Entitlement { EntitlementId = "starter-pack", Kind = "pack", IsActive = true });
    return Task.FromResult(response);
  }
}
