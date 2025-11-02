namespace UnstableCoreChallenge.Services;

public class UnstableCoreService(
    DefaultEntityTracker<UnstableCoreStabilizer> tracker,
    CoreDisarmService coreDisarmService,
    UnstableCoreSpawner spawner,
    EventBus eb    
) : ILoadableSingleton, ITickableSingleton
{
    float pollingCountdown;

    public StabilizerRecord InitializeNewCore() => coreDisarmService.Generate();

    public void Load()
    {
        tracker.OnEntityUnregistered += OnEntityUnregistered;
        eb.Register(this);
        PollEnsureEnoughCores();
    }

    void OnEntityUnregistered(UnstableCoreStabilizer obj)
    {
        PollEnsureEnoughCores();
    }

    [OnEvent]
    public void OnNewCycle(CycleStartedEvent _)
    {
        coreDisarmService.CalculateForThisCycle();
        PollEnsureEnoughCores();
    }

    public void PollEnsureEnoughCores()
    {
        if (pollingCountdown <= 0f)
        {
            pollingCountdown = 10f;
        }
    }

    public void Tick()
    {
        if (pollingCountdown <= 0f) { return; }

        pollingCountdown--;
        if (pollingCountdown <= 0f)
        {
            EnsureEnoughCores();
        }
    }

    void EnsureEnoughCores()
    {
        if (tracker.Entities.Count >= coreDisarmService.MaxCoreCount) { return; }

        spawner.SpawnCore();
        PollEnsureEnoughCores();
    }

}
