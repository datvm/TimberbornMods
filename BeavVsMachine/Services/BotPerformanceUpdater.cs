namespace BeavVsMachine.Services;

public class BotPerformanceUpdater(
    EventBus eb,
    BotPopulation botPopulation
) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        foreach (var b in botPopulation._bots)
        {
            b.GetComponentFast<BotAgingPerformanceComponent>().UpdatePerformance();
        }
    }

}
