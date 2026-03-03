namespace Thunderbird.GameRules.Session;

public static class SessionActionValidator
{
  public static bool IsActionEnvelopeValid(string actionId, string actionType, string rulesetVersion)
  {
    return !string.IsNullOrWhiteSpace(actionId)
      && !string.IsNullOrWhiteSpace(actionType)
      && !string.IsNullOrWhiteSpace(rulesetVersion);
  }
}
