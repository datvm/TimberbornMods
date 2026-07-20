namespace BuildingHP.Components;

public class HPMaintenanceComponentSpec : BaseComponent { }

public class HPMaintenanceComponent : TickableComponent, IPersistentEntity, IFinishedStateListener
{
    static readonly PropertyKey<float> NextMaintenanceKey = new("NextMaintenance");

#nullable disable
    BuildingHPComponent hpComponent;
    IDayNightCycle dayNightCycle;
#nullable enable

    float nextMaintenance;

    [Inject]
    public void Inject(IDayNightCycle dayNightCycle)
    {
        this.dayNightCycle = dayNightCycle;
    }

    public void Awake()
    {
        hpComponent = this.GetHPComponent();

        if (nextMaintenance <= 0)
        {
            SetNextMaintenanceTime();
        }

        enabled = false;
    }

    public override void Tick()
    {
        if (dayNightCycle.PartialDayNumber < nextMaintenance) { return; }

        hpComponent.Damage(1);
        SetNextMaintenanceTime();
    }

    void SetNextMaintenanceTime()
    {
        nextMaintenance = dayNightCycle.PartialDayNumber + MSettings.MaintenanceHPDaysValue;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(BuildingHPComponent.SaveKey, out var s)) { return; }

        if (s.Has(NextMaintenanceKey))
        {
            nextMaintenance = s.Get(NextMaintenanceKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(BuildingHPComponent.SaveKey);
        s.Set(NextMaintenanceKey, nextMaintenance);
    }

    public void OnEnterFinishedState()
    {
        enabled = true;
    }

    public void OnExitFinishedState()
    {
        enabled = false;
    }
}
