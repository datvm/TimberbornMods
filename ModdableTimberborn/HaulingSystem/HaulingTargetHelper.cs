namespace ModdableTimberborn.HaulingSystem;

/// <summary>
/// Resolves which district centers should be offered an extra-hauler job for a given
/// destination <see cref="Accessible"/> (construction sites, custom buildings, etc.).
/// </summary>
[BindSingleton]
public class HaulingTargetHelper(
    DistrictCenterRegistry districtCenterRegistry,
    InstantDistrictMap instantDistrictMap,
    NodeIdService nodeIdService
)
{
    /// <summary>
    /// Finds finished district centers that should serve hauling to <paramref name="accessible"/>.
    /// </summary>
    /// <param name="accessible">
    /// Destination access (e.g. <see cref="ConstructionSiteAccessible.Accessible"/>).
    /// Must be enabled with at least one access for spill-based resolution.
    /// </param>
    /// <param name="ignoreEntrance">
    /// When <c>false</c> (default): if the same entity has a <see cref="DistrictBuilding"/> with
    /// an assigned finished/construction district, return only that district (entrance-capable
    /// buildings). When <c>true</c>, skip that and only use instant road-spill membership
    /// (same idea as builder reachability for multi-access / no-entrance sites).
    /// </param>
    /// <returns>
    /// Zero or more finished <see cref="DistrictCenter"/>s. Empty if nothing can serve yet
    /// (no district assigned and/or site not on any spill). Callers that need
    /// <c>Count</c> can materialize (e.g. <c>ToList()</c> / <c>ToArray()</c>).
    /// </returns>
    public IEnumerable<DistrictCenter> FindDistrictsFor(
        Accessible accessible,
        bool ignoreEntrance = false)
    {
        if (!accessible)
        {
            yield break;
        }

        if (!ignoreEntrance)
        {
            var districtBuilding = accessible.GetComponent<DistrictBuilding>();
            if (districtBuilding)
            {
                var assigned = districtBuilding.GetDistrictOrConstructionDistrict();
                if (assigned)
                {
                    yield return assigned;
                    yield break;
                }

                // Has DistrictBuilding but not assigned yet — wait (do not spill-fallback
                // unless caller passes ignoreEntrance: true).
                yield break;
            }
        }

        foreach (var center in FindDistrictsByRoadSpill(accessible))
        {
            yield return center;
        }
    }

    /// <summary>
    /// Districts whose instant road-spill covers any access point of <paramref name="accessible"/>.
    /// Uses precomputed maps (same family as builder construction reachability).
    /// </summary>
    public IEnumerable<DistrictCenter> FindDistrictsByRoadSpill(Accessible accessible)
    {
        if (!accessible || !accessible.Enabled || accessible.Accesses.Count == 0)
        {
            yield break;
        }

        foreach (var center in districtCenterRegistry.FinishedDistrictCenters)
        {
            if (!center || center.District is null)
            {
                continue;
            }

            if (IsAccessibleOnDistrictSpill(center.District, accessible))
            {
                yield return center;
            }
        }
    }

    public bool IsAccessibleOnDistrictSpill(District district, Accessible accessible)
    {
        if (!accessible || district is null)
        {
            return false;
        }

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
