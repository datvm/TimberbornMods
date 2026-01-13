
namespace BlueprintRelics.Services.Rewards;

public class ScienceReward(int science, ScienceService scienceService) : IRelicReward
{
    public string Title { get; }

    public void Apply()
    {
        throw new NotImplementedException();
    }
}
