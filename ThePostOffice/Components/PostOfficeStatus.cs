namespace ThePostOffice.Components;

public enum PostOfficeStall
{
    None,
    NothingToDo,
    NoStorage,
    LackOfResources,
}

public class PostOfficeStatus(ILoc t) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity
{
    const float DelayHours = 0.25f;

    readonly List<Inventory> listened = [];

    StatusToggle nothing = null!;
    StatusToggle storage = null!;
    StatusToggle resources = null!;
    PostOfficeSender? sender;
    PostOfficeReceiver? receiver;
    DistrictBuilding? districtBuilding;
    DistrictInventoryRegistry? registry;
    PostOfficeStall shown = PostOfficeStall.None;
    bool ready;

    public void Awake()
    {
        TryGetComponent(out sender);
        TryGetComponent(out receiver);
        TryGetComponent(out districtBuilding);
    }

    public void InitializeEntity()
    {
        nothing = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "NothingToDo",
            t.T("LV.TPO.StatusNothing"),
            t.T("LV.TPO.StatusNothingShort"),
            DelayHours);
        storage = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "NoStorage",
            t.T("LV.TPO.StatusStorage"),
            t.T("LV.TPO.StatusStorageShort"),
            DelayHours);
        resources = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "LackOfResources",
            t.T("Status.Work.LackOfResources"),
            t.T("Status.Work.LackOfResources.Short"));
        var subject = GetComponent<StatusSubject>();
        subject.RegisterStatus(nothing);
        subject.RegisterStatus(storage);
        subject.RegisterStatus(resources);
        ready = true;
        var building = districtBuilding;
        if (building is not null && sender is not null)
        {
            building.ReassignedDistrict += OnDistrict;
            Bind(building.District);
        }

        Refresh();
    }

    public void DeleteEntity()
    {
        var building = districtBuilding;
        if (building is not null)
        {
            building.ReassignedDistrict -= OnDistrict;
        }

        Unbind();
    }

    public void Refresh()
    {
        if (!ready)
        {
            return;
        }

        var next = Current();
        if (next == shown)
        {
            return;
        }

        shown = next;
        nothing.Toggle(next == PostOfficeStall.NothingToDo);
        storage.Toggle(next == PostOfficeStall.NoStorage);
        resources.Toggle(next == PostOfficeStall.LackOfResources);
    }

    PostOfficeStall Current()
    {
        var sending = sender;
        if (sending is not null && sending)
        {
            return sending.Stall();
        }

        var receiving = receiver;
        if (receiving is not null && receiving)
        {
            return receiving.Stall();
        }

        return PostOfficeStall.None;
    }

    void OnDistrict(object sender, EventArgs e)
    {
        var building = districtBuilding;
        Bind(building is not null ? building.District : null);
        Refresh();
    }

    void Bind(DistrictCenter? center)
    {
        Unbind();
        if (center is not DistrictCenter currentCenter || !currentCenter)
        {
            return;
        }

        registry = currentCenter.GetComponent<DistrictInventoryRegistry>();
        var current = registry;
        if (!current)
        {
            registry = null;
            return;
        }

        current.InventoryRegistered += OnRegistered;
        current.InventoryUnregistered += OnUnregistered;
        foreach (var inventory in current.Inventories)
        {
            Listen(inventory);
        }
    }

    void Unbind()
    {
        var current = registry;
        if (current is not null)
        {
            current.InventoryRegistered -= OnRegistered;
            current.InventoryUnregistered -= OnUnregistered;
            registry = null;
        }

        foreach (var inventory in listened)
        {
            if (inventory)
            {
                inventory.InventoryStockChanged -= OnDistrictStock;
            }
        }

        listened.Clear();
    }

    void OnRegistered(object sender, Inventory inventory) => Listen(inventory);

    void OnUnregistered(object sender, Inventory inventory)
    {
        if (!listened.Remove(inventory))
        {
            return;
        }

        if (inventory)
        {
            inventory.InventoryStockChanged -= OnDistrictStock;
        }

        Refresh();
    }

    void Listen(Inventory inventory)
    {
        if (!inventory || listened.Contains(inventory))
        {
            return;
        }

        inventory.InventoryStockChanged += OnDistrictStock;
        listened.Add(inventory);
    }

    void OnDistrictStock(object sender, InventoryStockChangedEventArgs e) => Refresh();
}
