namespace BlueprintRelics.Services.Rewards;

public interface IRewardGenerator
{

    BlueprintRelicSize ForSize { get; }
    IRelicReward[] GenerateRewards(BlueprintRelicCollector collector, BlueprintRelicRecipeUpgradeSpec spec);

}

public abstract class RewardGeneratorBase : IRewardGenerator
{
    public abstract BlueprintRelicSize ForSize { get; }
    public abstract IRelicReward[] GenerateRewards(BlueprintRelicCollector collector, BlueprintRelicRecipeUpgradeSpec spec);
}