using Thunderbird.GameRules.Offline;

namespace Thunderbird.Api.Tests;

public class OfflineMergePolicyTests
{
  [Fact]
  public void CanMerge_AcceptsAllowedClasses()
  {
    Assert.True(OfflineMergePolicy.CanMerge(OfflineDeltaClass.Cosmetic));
    Assert.True(OfflineMergePolicy.CanMerge(OfflineDeltaClass.UnrankedProgress));
    Assert.True(OfflineMergePolicy.CanMerge(OfflineDeltaClass.NonEconomyUnlock));
  }

  [Fact]
  public void CanMerge_RejectsRankedAndEconomyClasses()
  {
    Assert.False(OfflineMergePolicy.CanMerge(OfflineDeltaClass.RankedMmr));
    Assert.False(OfflineMergePolicy.CanMerge(OfflineDeltaClass.CompetitiveOutcome));
    Assert.False(OfflineMergePolicy.CanMerge(OfflineDeltaClass.PremiumCurrency));
  }
}
