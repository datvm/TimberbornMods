namespace ModdableTimberborn.BonusSystem;

// 1.1 removed IStartableComponent. This component's only start-time work is replaying bonuses loaded from
// the save (the 1.0 Start() early-returned when nothing was loaded), so the precise 1.1 hook is
// IPostLoadableEntity.PostLoadEntity() — guaranteed to run after Load(), unlike the init phases.
public class PersistentBonusTrackerComponent : BaseComponent, IPersistentEntity, IBonusTrackerComponent, IAwakableComponent,
#if TIMBERV11
    IPostLoadableEntity
#else
    IStartableComponent
#endif
{
    static readonly ComponentKey SaveKey = new(nameof(PersistentBonusTrackerComponent));
    static readonly ListKey<BonusTrackerItem> CurrentBonusesKey = new("CurrentBonuses");

    List<BonusTrackerItem>? pending;
    public BonusTracker BonusTracker { get; private set; } = null!;

    public void Awake()
    {
        var bm = this.GetBonusManager();
        BonusTracker = new(bm);
    }

#if TIMBERV11
    public void PostLoadEntity()
#else
    public void Start()
#endif
    {
        if (pending is null) { return; }

        foreach (var item in pending)
        {
            BonusTracker.AddOrUpdate(item);
        }
        pending = null;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        pending = s.Get(CurrentBonusesKey, BonusTrackerItemSerializer.Instance);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (BonusTracker.CurrentBonuses.Count == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(CurrentBonusesKey, [..BonusTracker.CurrentBonuses.Values], BonusTrackerItemSerializer.Instance);
    }

}
