namespace BuildingHP.Components.Renovations;

public class ReinforceGearComponent : TogglableWorkplaceBonusComponent, IBuildingDeltaDurabilityModifier, IActivableRenovationComponent, IWorkplaceWorkerEffectDescriber
{
    public const string BonusId = "Renovation.ReinforceGearWork";

#nullable disable
    RenovationSpec spec;
#nullable enable

    bool hasWorkplace;

    public bool RenovationActive { get; private set; }
    public int? Delta { get; private set; }
    public string DescriptionKey { get; } = "LV.BHP.ReinforceGear";
    public float? ModifierEndTime { get; }

    public Action<BuildingRenovation>? ActiveHandler { get; set; }
    public HashSet<BonusManager> WorkerBonus { get; } = [];
    public float ProductivityMultiplier { get; set; }
    public EventHandler<WorkerChangedEventArgs>? WorkerAssigned { get; set; }
    public EventHandler<WorkerChangedEventArgs>? WorkerUnassigned { get; set; }

    protected override BonusTrackerItem Bonuses => new(BonusId, BonusType.WorkingSpeed, spec.Parameters[1]);

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    public void Initialize()
    {
        hasWorkplace = GetComponentFast<Workplace>();

        var reno = this.GetRenovationComponent();
        spec = reno.RenovationService.GetSpec(ReinforceGearRenovationProvider.RenovationId);

        this.ActivateIfAvailable(ReinforceGearRenovationProvider.RenovationId);
    }

    public void Activate()
    {
        RenovationActive = true;

        ActivateDurability();

        if (hasWorkplace)
        {
            Toggle(true);
        }
    }

    void ActivateDurability()
    {
        Delta = (int)spec.Parameters[0];
        OnChanged?.Invoke(this);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (!RenovationActive) { return null; }

        var desc = t.T("LV.BHP.ReinforceGearEffDurability", spec.Parameters[0]);
        if (hasWorkplace)
        {
            desc += Environment.NewLine + t.T("LV.BHP.ReinforceGearEffProductivity", spec.Parameters[1]);
        }

        return RenovationActive
            ? new(spec.Title.Value, desc)
            : null;
    }

    public EntityEffectDescription? DescribeWorkerEffect(Worker worker, ILoc t, IDayNightCycle dayNightCycle)
    {
        if (!RenovationActive || !hasWorkplace) { return null; }

        return new(spec.Title.Value, t.T("LV.BHP.ReinforceGearWorkerEff", spec.Parameters[1]));
    }
}
