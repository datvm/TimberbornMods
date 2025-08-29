namespace BuildingHP.Components;

public class BuildingRenovationStockpileComponent : BaseComponent, IFinishedStateListener, IPrioritizable, IPersistentEntity
{
    public static readonly ComponentKey SaveKey = new("BuildingHPStockpile");
    static readonly PropertyKey<bool> SupplyKey = new("Supply");
    static readonly PropertyKey<int> PriorityKey = new("Priority");

#nullable disable
    public Stockpile Stockpile { get; private set; }
    BuildingHPRegistry buildingHPRegistry;
#nullable enable

    public bool Supply { get; private set; }
    public Priority Priority { get; private set; } = Priority.Normal;

    [Inject]
    public void Inject(BuildingHPRegistry buildingHPRegistry)
    {
        this.buildingHPRegistry = buildingHPRegistry;
    }

    public void Awake()
    {
        Stockpile = GetComponentFast<Stockpile>();
    }

    void Reregister()
    {
        if (Supply)
        {
            buildingHPRegistry.RegisterStockpile(this, Priority);
        }
        else
        {
            buildingHPRegistry.UnregisterStockpile(this);
        }
    }

    public void OnEnterFinishedState()
    {

    }

    public void OnExitFinishedState()
    {
        buildingHPRegistry.UnregisterStockpile(this);
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
        Reregister();
    }

    public void SetSupply(bool supply)
    {
        Supply = supply;
        Reregister();
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(SupplyKey, Supply);
        s.Set(PriorityKey, (int)Priority);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(SupplyKey))
        {
            Supply = s.Get(SupplyKey);
        }
        
        if (s.Has(PriorityKey))
        {
            Priority = (Priority)s.Get(PriorityKey);
        }

        Reregister();
    }
}
