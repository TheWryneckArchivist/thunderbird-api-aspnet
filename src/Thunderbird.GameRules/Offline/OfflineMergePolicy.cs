namespace Thunderbird.GameRules.Offline;

public enum OfflineDeltaClass
{
  Cosmetic,
  UnrankedProgress,
  NonEconomyUnlock,
  RankedMmr,
  CompetitiveOutcome,
  PremiumCurrency
}

public static class OfflineMergePolicy
{
  public static bool CanMerge(OfflineDeltaClass deltaClass)
  {
    return deltaClass is
      OfflineDeltaClass.Cosmetic or
      OfflineDeltaClass.UnrankedProgress or
      OfflineDeltaClass.NonEconomyUnlock;
  }
}
