namespace ConstructionSiteHauler.Services;

/// <summary>
/// Extra-hauler registration for construction sites and global refresh on instant nav /
/// district registry changes. District resolution is delegated to
/// <see cref="HaulingTargetHelper"/>. Weight comes from construction
/// <see cref="BuilderPrioritizable"/> priority only (fixed while the site needs goods).
/// </summary>
[BindSingleton]
public class ConstructionSiteHaulerService(
    ExtraHaulerTargetService extraHaulerTargets,
    HaulingTargetHelper haulingTargetHelper,
    DefaultEntityTracker<ConstructionSiteHaulerComponent> csHaulers,
    EventBus eventBus
) : ILoadableSingleton, ISingletonInstantNavMeshListener
{
    public void Load()
    {
        eventBus.Register(this);
    }

    public void OnInstantNavMeshUpdated(NavMeshUpdate navMeshUpdate) => RefreshAll();

    [OnEvent]
    public void OnDistrictCenterRegistryChanged(DistrictCenterRegistryChangedEvent e)
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        foreach (var hauler in csHaulers.Entities)
        {
            Refresh(hauler);
        }
    }

    public void Refresh(ConstructionSiteHaulerComponent site)
    {
        Unregister(site);

        var inventory = site.Inventory;
        if (!inventory || !inventory.Enabled || inventory.IsFull)
        {
            return;
        }

        if (!site.ConstructionSiteAccessible)
        {
            return;
        }

        var accessible = site.ConstructionSiteAccessible!.Accessible;
        if (!accessible)
        {
            return;
        }

        var districts = haulingTargetHelper.FindDistrictsFor(accessible).ToArray();
        if (districts.Length == 0)
        {
            return;
        }

        site.Registration = extraHaulerTargets.AddExtraTarget(new ExtraHaulerTargetRegistration(
            Inventory: inventory,
            Districts: districts,
            Accessible: accessible,
            Weight: ComputeHaulWeight(site.Priority),
            OnlyInputGoods: true));
    }

    public void Unregister(ConstructionSiteHaulerComponent site)
    {
        site.Registration?.Dispose();
        site.Registration = null;
    }

    /// <summary>
    /// Fixed weight from construction priority only (not fill %).
    /// Workshop empty fill ≈ 1.0; Normal+ stays above that for the whole build so haulers
    /// keep feeding sites instead of abandoning them for empty workshops.
    /// </summary>
    public static float ComputeHaulWeight(Priority priority)
        => priority switch
        {
            Priority.VeryLow => 0.25f,
            Priority.Low => 0.5f,
            Priority.Normal => 1.1f,
            Priority.High => 1.5f,
            Priority.VeryHigh => 2f,
            _ => 1.1f,
        };
}
