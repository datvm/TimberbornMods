namespace BlueprintRelics.Services.Rewards;

public interface IRewardGenerator
{

    BlueprintRelicSize Size { get; }
    IRelicReward[] GenerateRewards(BlueprintRelicCollector collector);

}
