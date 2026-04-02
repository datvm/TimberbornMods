namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class NoFloodForCyclesAchievement(
    DefaultEntityTracker<FloodableObject> tracker,
    EventBus eb,
    ISingletonLoader loader
) : EbAchievementBase(eb), ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(NoFloodForCyclesAchievement));
    static readonly PropertyKey<int> NoFloodCycleKey = new("NoFloodCycle");

    public const string AchId = "LV.MA.NoFloodForCycles";
    public const int RequiredCycles = 3;

    public override string Id => AchId;

    readonly HashSet<FloodableObject> trackingBuildings = [];
    bool HasAnyFloodedBuildings => trackingBuildings.Count > 0 && trackingBuildings.Any(b => b.IsFlooded);

    public int NoFloodCycles { get; private set; }

    public override void EnableInternal()
    {
        base.EnableInternal();
        tracker.OnEntityRegistered += RegisterEntity;
        tracker.OnEntityUnregistered += UnregisterEntity;
        foreach (var e in tracker.Entities)
        {
            RegisterEntity(e);
        }
    }

    void RegisterEntity(FloodableObject obj)
    {
        if (!obj.HasComponent<FloodableBuilding>()
            || obj.HasComponent<BlockableFloodableObjectSpec>()) { return; }

        trackingBuildings.Add(obj);
        obj.Flooded += OnFlooded;

        if (obj.IsFlooded) { OnFlooded(); }
    }

    void OnFlooded(object sender, EventArgs e) => OnFlooded();

    void UnregisterEntity(FloodableObject obj)
    {
        obj.Flooded -= OnFlooded;
        trackingBuildings.Remove(obj);
    }

    public override void DisableInternal()
    {
        base.DisableInternal();
        tracker.OnEntityRegistered -= RegisterEntity;
        tracker.OnEntityUnregistered -= UnregisterEntity;

        foreach(var e in trackingBuildings)
        {
            e.Flooded -= OnFlooded;
        }
        trackingBuildings.Clear();

        NoFloodCycles = 0;
    }

    void OnFlooded()
    {
        NoFloodCycles = 0;
    }

    [OnEvent]
    public void OnNewCycle(CycleStartedEvent e)
    {
        if (e.Cycle == 1) { return; } // First cycle does not count

        if (!HasAnyFloodedBuildings)
        {
            NoFloodCycles++;

            if (NoFloodCycles >= RequiredCycles)
            {
                Unlock();
            }
        }
        else
        {
            NoFloodCycles = 0;
        }
    }

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(NoFloodCycleKey))
        {
            NoFloodCycles = s.Get(NoFloodCycleKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (NoFloodCycles == 0) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(NoFloodCycleKey, NoFloodCycles);
    }

}
