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
    public ConstructionSite ConstructionSite { get; private set; } = null!;
    public DistrictBuilding? DistrictBuilding { get; private set; }
    public ConstructionSiteAccessible? ConstructionSiteAccessible { get; private set; }
    public PausableBuilding? PausableBuilding { get; private set; }

    BuilderPrioritizable? builderPrioritizable;
    public Priority Priority => builderPrioritizable?.Priority ?? Priority.Normal;

    /// <summary>True when the unfinished site is paused (no hauler materials).</summary>
    public bool IsPaused => PausableBuilding && PausableBuilding!.Paused;

    /// <summary>Active extra-hauler registration for this site, if any.</summary>
    internal IDisposable? Registration { get; set; }

    public void InitializeEntity()
    {
        ConstructionSite = GetComponent<ConstructionSite>();
        Inventory = ConstructionSite.Inventory;
        DistrictBuilding = GetComponent<DistrictBuilding>();
        ConstructionSiteAccessible = GetComponent<ConstructionSiteAccessible>();
        builderPrioritizable = GetComponent<BuilderPrioritizable>();
        PausableBuilding = GetComponent<PausableBuilding>();

        Inventory.InventoryChanged += OnInventoryChanged;
        if (DistrictBuilding)
        {
            DistrictBuilding.ReassignedConstructionDistrict += OnDistrictChanged;
            DistrictBuilding.ReassignedDistrict += OnDistrictChanged;
        }

        if (builderPrioritizable)
        {
            builderPrioritizable.PriorityChanged += OnPriorityChanged;
        }

        if (PausableBuilding)
        {
            PausableBuilding!.PausedChanged += OnPausedChanged;
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

    void OnPriorityChanged(object sender, PriorityChangedEventArgs e)
    {
        service.Refresh(this);
    }

    void OnPausedChanged(object sender, EventArgs e)
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

        if (builderPrioritizable)
        {
            builderPrioritizable!.PriorityChanged -= OnPriorityChanged;
        }

        if (PausableBuilding)
        {
            PausableBuilding!.PausedChanged -= OnPausedChanged;
        }
    }
}
