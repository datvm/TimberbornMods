global using ModdableTimberborn.GameStats;

namespace UnstableCoreChallenge.Services;

[BindSingleton]
public class UnstableCoreStabilizerService(
    DefaultEntityTracker<UnstableCoreStabilizer> tracker,
    UnstableCoreSpawner spawner,
    EventBus eb,
    GameStatService gameStats,
    EntityService entityService
) : ILoadableSingleton, ITickableSingleton
{
    int prevHours = -1;

    public void Load()
    {
        eb.Register(this);
    }

    public void Tick()
    {
        var hours = (int)gameStats.GetStat<float>(GameStats.TimeTodayHours);
        if (hours == prevHours) { return; }

        prevHours = hours;
        TrySpawningCore();
    }

    void TrySpawningCore()
    {

    }

    public void Delete(UnstableCoreStabilizer stabilizer)
    {
        entityService.Delete(stabilizer);
    }

}
