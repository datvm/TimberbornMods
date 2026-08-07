namespace ConstructionSiteHauler.Components;

/// <summary>
/// Thin hook on each construction site. Registration handle lives here;
/// resolution logic is in <see cref="ConstructionSiteHaulerService"/>.
/// </summary>
[AddTemplateModule2(typeof(ConstructionSite))]
public class ConstructionSiteHaulerComponent(
    ConstructionSiteHaulerService service
) : BaseComponent, IInitializableEntity, IDeletableEntity, IUnfinishedStateListener
{
    public Inventory Inventory { get; private set; } = null!;
    public DistrictBuilding? DistrictBuilding { get; private set; }
    public ConstructionSiteAccessible? ConstructionSiteAccessible { get; private set; }

    /// <summary>Active extra-hauler registration for this site, if any.</summary>
    internal IDisposable? Registration { get; set; }

    public void InitializeEntity()
    {
        Inventory = GetComponent<ConstructionSite>().Inventory;
        DistrictBuilding = GetComponent<DistrictBuilding>();
        ConstructionSiteAccessible = GetComponent<ConstructionSiteAccessible>();

        Inventory.InventoryChanged += OnInventoryChanged;
        if (DistrictBuilding)
        {
            DistrictBuilding.ReassignedConstructionDistrict += OnDistrictChanged;
            DistrictBuilding.ReassignedDistrict += OnDistrictChanged;
        }

        service.Refresh(this);
    }

    public void DeleteEntity()
    {
        DetachHandlers();
        service.Unregister(this);
    }

    public void OnEnterUnfinishedState()
    {
        service.Refresh(this);
    }

    public void OnExitUnfinishedState()
    {
        service.Unregister(this);
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        service.Refresh(this);
    }

    void OnDistrictChanged(object sender, EventArgs e)
    {
        service.Refresh(this);
    }

    void DetachHandlers()
    {
        if (Inventory)
        {
            Inventory.InventoryChanged -= OnInventoryChanged;
        }

        if (DistrictBuilding)
        {
            DistrictBuilding!.ReassignedConstructionDistrict -= OnDistrictChanged;
            DistrictBuilding.ReassignedDistrict -= OnDistrictChanged;
        }
    }
}
