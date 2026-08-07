namespace ConstructionSiteHauler.Services;

/// <summary>
/// Extra-hauler registration logic for construction sites: spill-based district
/// resolution and global refresh on instant nav / district registry changes.
/// Per-site registration handles live on <see cref="ConstructionSiteHaulerComponent"/>.
/// </summary>
[BindSingleton]
public class ConstructionSiteHaulerService(
    ExtraHaulerTargetService extraHaulerTargets,
    DistrictCenterRegistry districtCenterRegistry,
    InstantDistrictMap instantDistrictMap,
    NodeIdService nodeIdService,
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

        var districts = ResolveDistricts(site);
        if (districts.Count == 0)
        {
            return;
        }

        Accessible? accessible = null;
        if (site.ConstructionSiteAccessible)
        {
            accessible = site.ConstructionSiteAccessible!.Accessible;
        }

        site.Registration = extraHaulerTargets.AddExtraTarget(new ExtraHaulerTargetRegistration(
            Inventory: inventory,
            Districts: districts,
            Accessible: accessible,
            Weight: 1f,
            OnlyInputGoods: true));
    }

    public void Unregister(ConstructionSiteHaulerComponent site)
    {
        site.Registration?.Dispose();
        site.Registration = null;
    }

    /// <summary>
    /// Prefer assigned construction/finished district when present; otherwise districts
    /// whose instant road-spill covers any of the site's construction accesses
    /// (precomputed maps — same family as builder reachability / district range).
    /// </summary>
    List<DistrictCenter> ResolveDistricts(ConstructionSiteHaulerComponent site)
    {
        if (site.DistrictBuilding)
        {
            var assigned = site.DistrictBuilding!.GetDistrictOrConstructionDistrict();
            if (assigned)
            {
                return [assigned];
            }
        }

        return FindDistrictsByRoadSpill(site);
    }

    List<DistrictCenter> FindDistrictsByRoadSpill(ConstructionSiteHaulerComponent site)
    {
        if (!site.ConstructionSiteAccessible)
        {
            return [];
        }

        var accessible = site.ConstructionSiteAccessible!.Accessible;
        if (!accessible || !accessible.Enabled || accessible.Accesses.Count == 0)
        {
            return [];
        }

        List<DistrictCenter> result = [];
        foreach (var center in districtCenterRegistry.FinishedDistrictCenters)
        {
            if (!center || center.District is null)
            {
                continue;
            }

            if (IsAccessibleOnDistrictSpill(center.District, accessible))
            {
                result.Add(center);
            }
        }

        return result;
    }

    bool IsAccessibleOnDistrictSpill(District district, Accessible accessible)
    {
        foreach (var access in accessible.Accesses)
        {
            if (!nodeIdService.Contains(access))
            {
                continue;
            }

            var nodeId = nodeIdService.WorldToId(access);
            if (instantDistrictMap.TryGetParentRoadNode(district, nodeId, out _))
            {
                return true;
            }
        }

        return false;
    }
}
