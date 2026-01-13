namespace BlueprintRelics.Services.Rewards;

public interface IRelicReward
{
    string Title { get; }
    void Apply();
}
