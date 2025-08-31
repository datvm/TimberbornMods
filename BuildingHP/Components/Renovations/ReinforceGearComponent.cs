namespace BuildingHP.Components.Renovations;

public class ReinforceGearComponent : BaseComponent, IBuildingDeltaDurabilityModifier, IActivableRenovationComponent, IWorkplaceProductivityComponent, IActiveRenovationDescriber
{

#nullable disable
    RenovationSpec spec;
#nullable enable

    bool hasWorkplace;

    public bool Active { get; private set; }
    public int? Delta { get; private set; }
    public string DescriptionKey { get; } = "LV.BHP.ReinforceGear";
    public float? ModifierEndTime { get; }

    public Action<BuildingRenovation>? ActiveHandler { get; set; }
    public HashSet<BonusManager> WorkerBonus { get; } = [];
    public float ProductivityMultiplier { get; set; }
    public EventHandler<WorkerChangedEventArgs>? WorkerAssigned { get; set; }
    public EventHandler<WorkerChangedEventArgs>? WorkerUnassigned { get; set; }

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    public void Start()
    {
        hasWorkplace = GetComponentFast<Workplace>();

        var reno = this.GetRenovationComponent();
        spec = reno.RenovationService.GetSpec(ReinforceGearRenovationProvider.RenovationId);
    }

    public void Initialize()
    {
        this.ActivateIfAvailable(ReinforceGearRenovationProvider.RenovationId);
    }

    public void Activate()
    {
        Active = true;

        var comp = this.GetRenovationComponent();
        var spec = comp.RenovationService.GetSpec(ReinforceGearRenovationProvider.RenovationId);
        
        ActivateDurability(spec);

        if (hasWorkplace)
        {
            this.SetWorkplaceProductivity(spec.Parameters[1]);
        }
    }

    void ActivateDurability(RenovationSpec spec)
    {
        Delta = (int)spec.Parameters[0];
        OnChanged?.Invoke(this);
    }

    public ActiveRenovationDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => Active
            ? new(spec.Title.Value, spec.Description)
            : null;
}
