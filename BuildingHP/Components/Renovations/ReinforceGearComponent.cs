namespace BuildingHP.Components.Renovations;

public class ReinforceGearComponent : BaseComponent, IBuildingDeltaDurabilityModifier
{
    const string WorkBonusId = "WorkingSpeed";

    public bool Active { get; private set; }
    public int? Delta { get; private set; }
    public string DescriptionKey { get; } = "LV.BHP.ReinforceGear";
    public float? ModifierEndTime { get; }

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    Workplace? workplace;
    float workplaceBonus;

    public void Initialize()
    {
        ActivateIfAvailable(true);
    }

    void ActivateIfAvailable(bool listenIfNotActive)
    {
        if (Active) { return; }

        var comp = this.GetRenovationComponent();
        Active = comp.HasRenovation(ReinforceGearRenovationProvider.RenovationId);

        if (Active)
        {
            comp.RenovationCompleted -= OnRenovationCompleted;
            Activate();
        }
        else if (listenIfNotActive)
        {
            comp.RenovationCompleted += OnRenovationCompleted;
        }
    }

    private void OnRenovationCompleted(BuildingRenovation obj)
    {
        ActivateIfAvailable(false);
    }

    void Activate()
    {
        Active = true;

        var comp = this.GetRenovationComponent();
        var spec = comp.RenovationService.GetSpec(ReinforceGearRenovationProvider.RenovationId);
        workplaceBonus = spec.Parameters[1];

        ActivateDurability(spec);
        ActivateWorkplace();
    }

    void ActivateWorkplace()
    {
        workplace = GetComponentFast<Workplace>();
        if (!workplace) { return; }

        workplace.WorkerAssigned += OnWorkerAssigned;
        workplace.WorkerUnassigned += OnWorkerUnassigned;

        foreach (var w in workplace.AssignedWorkers)
        {
            AddBonus(w);
        }
    }

    private void OnWorkerUnassigned(object sender, WorkerChangedEventArgs e)
    {
        e.Worker.GetComponentFast<BonusManager>().RemoveBonus(WorkBonusId, workplaceBonus);
    }

    private void OnWorkerAssigned(object sender, WorkerChangedEventArgs e)
    {
        AddBonus(e.Worker);
    }

    void AddBonus(Worker worker)
    {
        worker.GetComponentFast<BonusManager>().AddBonus(WorkBonusId, workplaceBonus);
    }

    void ActivateDurability(RenovationSpec spec)
    {
        Delta = (int)spec.Parameters[0];
        OnChanged?.Invoke(this);
    }

    
}
