namespace ModdableTimberborn.BonusSystem;

public class PersistentBonusTrackerComponent : BaseComponent, IPersistentEntity, IBonusTrackerComponent
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

    public void Start()
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
