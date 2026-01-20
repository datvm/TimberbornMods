namespace BlueprintRelics.Services.Rewards;

public class ScienceReward(int science, ScienceService scienceService) : IRelicReward
{
    public static readonly int[] ScienceMultiplier = [2, 4, 6];

    public string TitleLoc => "LV.BRe.RewardScience";
    public int Amount => science;

    public ScienceReward(BlueprintRelicCollector collector, ScienceService scienceService)
        : this(GetScienceRewardFrom(collector), scienceService) { }

    public void Apply()
    {
        scienceService.AddPoints(science);
    }

    public static int GetScienceRewardFrom(BlueprintRelicCollector collector) 
        => collector.ScienceRequirement * collector.TotalSteps * ScienceMultiplier[(int)collector.Size];

}
