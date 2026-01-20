namespace BlueprintRelics.Services.Rewards;

public interface IRelicReward
{
    string TitleLoc { get; }
    void Apply();
}
