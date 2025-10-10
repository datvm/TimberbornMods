namespace EarthquakeWeather.Components;

public class EarthquakeBuildingBlocker : TickableComponent, IPersistentEntity, IEntityEffectDescriber
{
    public const float BlockingTime = .5f;

    static readonly ComponentKey SaveKey = new(nameof(EarthquakeBuildingBlocker));
    static readonly PropertyKey<float> BlockingKey = new("Blocking");

    public float? BlockingUntil { get; private set; }

#nullable disable
    IDayNightCycle dayNightCycle;
    StatusToggle eqDamageStatus;
    BlockableBuilding blockableBuilding;
#nullable enable

    [Inject]
    public void Inject(IDayNightCycle dayNightCycle, ILoc t)
    {
        this.dayNightCycle = dayNightCycle;

        eqDamageStatus = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "building-damaged-eq",
            t.T("LV.EQ.DamageStatus"),
            t.T("LV.EQ.DamageStatusShort"));
    }

    public void Awake()
    {
        var eq = this.GetEarthquakeComponent();
        eq.OnAfterEarthquakeDamage += OnDamaged;

        blockableBuilding = GetComponentFast<BlockableBuilding>();
    }

    public override void StartTickable()
    {
        GetComponentFast<StatusSubject>().RegisterStatus(eqDamageStatus);
        enabled = false;

        if (BlockingUntil is not null)
        {
            Activate();
        }
    }

    private void OnDamaged(object sender, EarthquakeBuildingAfterDamageEventArgs e)
    {
        BlockingUntil = dayNightCycle.PartialDayNumber + BlockingTime;
        Activate();
    }

    void Activate()
    {
        eqDamageStatus.Activate();
        blockableBuilding.Block(this);

        enabled = true;
    }

    public override void Tick()
    {
        if (BlockingUntil > dayNightCycle.PartialDayNumber) { return; }

        BlockingUntil = null;
        eqDamageStatus.Deactivate();
        blockableBuilding.Unblock(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(BlockingKey))
        {
            BlockingUntil = s.Get(BlockingKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (BlockingUntil is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(BlockingKey, BlockingUntil.Value);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => BlockingUntil is null
            ? null
            : new(t.T("LV.EQ.EqBlocked"), t.T("LV.EQ.EqBlockedDesc"), BlockingUntil.Value - dayNightCycle.PartialDayNumber);
}
