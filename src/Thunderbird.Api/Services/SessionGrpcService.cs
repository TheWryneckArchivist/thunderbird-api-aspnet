using Grpc.Core;
using Thunderbird.Contracts.V1;

namespace Thunderbird.Api.Services;

public sealed class SessionGrpcService : SessionService.SessionServiceBase
{
  public override Task<CreateSessionResponse> CreateSession(CreateSessionRequest request, ServerCallContext context)
  {
    return Task.FromResult(new CreateSessionResponse
    {
      Session = BuildSessionSummary(Guid.NewGuid().ToString("N"), request.Title, request.RulesetVersion),
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<JoinSessionResponse> JoinSession(JoinSessionRequest request, ServerCallContext context)
  {
    return Task.FromResult(new JoinSessionResponse
    {
      Session = BuildSessionSummary(request.SessionId, "Joined Session", "v1"),
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<SubmitActionResponse> SubmitAction(SubmitActionRequest request, ServerCallContext context)
  {
    return Task.FromResult(new SubmitActionResponse
    {
      Accepted = true,
      ReasonCode = string.Empty,
      ServerTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<GetSessionStateResponse> GetSessionState(GetSessionStateRequest request, ServerCallContext context)
  {
    return Task.FromResult(new GetSessionStateResponse
    {
      SessionState = new SessionState
      {
        SessionId = request.SessionId,
        StateJson = "{}",
        ServerTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        RulesetVersion = "v1"
      },
      Meta = CorrelationMetaFactory.Create(context)
    });
  }

  public override Task<ListOpenSessionsResponse> ListOpenSessions(ListOpenSessionsRequest request, ServerCallContext context)
  {
    var response = new ListOpenSessionsResponse
    {
      Page = new PageResponse { HasMore = false, NextCursor = string.Empty },
      Meta = CorrelationMetaFactory.Create(context)
    };

    response.Sessions.Add(BuildSessionSummary("session-001", "Thunderbird Open Table", "v1"));
    return Task.FromResult(response);
  }

  private static SessionSummary BuildSessionSummary(string sessionId, string title, string rulesetVersion)
  {
    return new SessionSummary
    {
      SessionId = sessionId,
      Title = title,
      RulesetVersion = rulesetVersion,
      Status = "open",
      PlayerCount = 1,
      UpdatedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };
  }
}
