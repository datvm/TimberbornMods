namespace BuildingHP.Components.Renovations;

public class ProductOverdriveComponent : TickableComponent, IPersistentEntity, IWorkplaceWorkerEffectDescriber
{
    static readonly ComponentKey SaveKey = new("ProductOverdrive");
    static readonly PropertyKey<bool> CoolingDownKey = new("CoolingDown");
    static readonly PropertyKey<float> ProgressKey = new("Progress");

    ITimeTrigger? timeTrigger;
    bool isCoolingDown;
    float nextHpDrain;

    public float SpecOverdriveProductivity => spec.Parameters[0];
    public int SpecHpDrainPerHour => (int)spec.Parameters[1];
    public float SpecOverdriveDuration => spec.Parameters[2];
    public float SpecCooldownProductivity => spec.Parameters[3];
    public float SpecCooldownDuration => spec.Parameters[4];

    public bool Active => timeTrigger is not null;
    public bool IsInOverdrive => timeTrigger is not null && !isCoolingDown;
    public bool IsCoolingDown => timeTrigger is not null && isCoolingDown;

    public HashSet<BonusManager> WorkerBonus { get; } = [];
    public float ProductivityMultiplier { get; set; }
    public EventHandler<WorkerChangedEventArgs>? WorkerAssigned { get; set; }
    public EventHandler<WorkerChangedEventArgs>? WorkerUnassigned { get; set; }

    KeyValuePair<bool, float>? pendingLoad;
#nullable disable
    ITimeTriggerFactory timeTriggerFactory;
    IDayNightCycle dayNightCycle;
    RenovationSpec spec;
    BuildingHPComponent hp;
    ProductOverdriveBonusComponent bonusComponent;
#nullable enable

    [Inject]
    public void Inject(ITimeTriggerFactory timeTriggerFactory, IDayNightCycle dayNightCycle)
    {
        this.timeTriggerFactory = timeTriggerFactory;
        this.dayNightCycle = dayNightCycle;
    }

    public override void StartTickable()
    {
        bonusComponent = GetComponentFast<ProductOverdriveBonusComponent>();

        base.StartTickable();

        var reno = this.GetRenovationComponent();
        reno.RenovationCompleted += OnRenovationCompleted;
        spec = reno.RenovationService.GetSpec(ProductionOverdriveRenovationProvider.RenoId);

        hp = this.GetHPComponent();

        if (pendingLoad is not null)
        {
            PerformLoad();
        }

        if (!IsInOverdrive)
        {
            enabled = false;
        }
    }

    void PerformLoad()
    {
        var (loadCoolingDown, loadProgress) = pendingLoad!.Value;

        isCoolingDown = loadCoolingDown;
        if (isCoolingDown)
        {
            CreateCooldownEffect();
        }
        else
        {
            CreateOverdriveEffect();
        }

        timeTrigger!.FastForwardProgress(loadProgress);
    }

    void OnRenovationCompleted(BuildingRenovation obj)
    {
        if (obj.Id != ProductionOverdriveRenovationProvider.RenoId) { return; }
        CreateOverdriveEffect();
    }

    void CreateOverdriveEffect()
    {
        isCoolingDown = false;

        timeTrigger = timeTriggerFactory.Create(CreateCooldownEffect, SpecOverdriveDuration);
        timeTrigger.Resume();

        bonusComponent.Toggle(SpecOverdriveProductivity);
        ScheduleNextHpDrain();
        enabled = true;
    }

    void CreateCooldownEffect()
    {
        isCoolingDown = true;

        timeTrigger = timeTriggerFactory.Create(OnEffectCompleted, SpecCooldownDuration);
        timeTrigger.Resume();

        bonusComponent.Toggle(SpecCooldownProductivity);
    }

    void OnEffectCompleted()
    {
        timeTrigger = null;
        enabled = false;

        var reno = this.GetRenovationComponent();
        reno.RemoveActiveRenovation(ProductionOverdriveRenovationProvider.RenoId);

        bonusComponent.Toggle(null);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (timeTrigger is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ProgressKey, timeTrigger.Progress);
        s.Set(CoolingDownKey, isCoolingDown);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        isCoolingDown = s.Has(CoolingDownKey) && s.Get(CoolingDownKey);
        var progress = s.Has(ProgressKey) ? s.Get(ProgressKey) : 0f;

        pendingLoad = new(isCoolingDown, progress);
    }

    public override void Tick()
    {
        if (!IsInOverdrive)
        {
            enabled = false;
            return;
        }

        if (dayNightCycle.PartialDayNumber < nextHpDrain) { return; }
        hp.Damage(SpecHpDrainPerHour);
        ScheduleNextHpDrain();
    }

    void ScheduleNextHpDrain()
    {
        nextHpDrain = dayNightCycle.PartialDayNumber + 1 / 24f;
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (!Active) { return null; }

        var remainingTime = timeTrigger!.DaysLeft;
        return !isCoolingDown
            ? new(t.T("LV.BHP.ProdOverdriveFirstPhase"), t.T("LV.BHP.ProdOverdriveFirstPhaseDesc", SpecOverdriveProductivity, SpecHpDrainPerHour), remainingTime)
            : new(t.T("LV.BHP.ProdOverdriveSecondPhase"), t.T("LV.BHP.ProdOverdriveSecondPhaseDesc", SpecCooldownProductivity), remainingTime);
    }

    public EntityEffectDescription? DescribeWorkerEffect(Worker worker, ILoc t, IDayNightCycle dayNightCycle)
    {
        if (!Active) { return null; }

        var remainingTime = timeTrigger!.DaysLeft;
        return !isCoolingDown
            ? new(t.T("LV.BHP.ProdOverdriveFirstPhase"), t.T("LV.BHP.ProdOverdriveWorkerDesc", SpecOverdriveProductivity), remainingTime)
            : new(t.T("LV.BHP.ProdOverdriveSecondPhase"), t.T("LV.BHP.ProdOverdriveWorkerDesc", SpecCooldownProductivity), remainingTime);
    }
}
