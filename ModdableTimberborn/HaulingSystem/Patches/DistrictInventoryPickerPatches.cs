namespace ModdableTimberborn.HaulingSystem.Patches;

/// <summary>
/// Vanilla <see cref="DistrictInventoryPicker.ClosestInventoryWithStock(Accessible, string, Predicate{Inventory})"/>
/// paths <b>from</b> <c>start</c> via <see cref="Accessible.FindRoadPath(Accessible, out float)"/>, which uses
/// <c>FindRoadPathCached</c> and requires a road flow-field cache at the start access.
/// <list type="bullet">
/// <item>Multi-access starts (platforms etc.) also break on <see cref="Accessible.UnblockedSingleAccess"/> / <c>.Single()</c>.</item>
/// <item>Unfinished construction sites never run <c>BuildingCachingFlowField</c>, so even single-access sites throw
/// <c>InvalidOperationException: There's no cached flow field</c>.</item>
/// </list>
/// For multi-access starts and registered extra-hauler destinations we reverse the path
/// (warehouse → site), matching how builders pick stock, and fall back when a warehouse
/// entrance is not cached.
/// </summary>
[HarmonyPatch, HarmonyPatchCategory(ExtraHaulerTargetConfig.PatchCategoryName)]
public static class DistrictInventoryPickerPatches
{
    [HarmonyPrefix, HarmonyPatch(
        typeof(DistrictInventoryPicker),
        nameof(DistrictInventoryPicker.ClosestInventoryWithStock),
        [typeof(Accessible), typeof(string), typeof(Predicate<Inventory>)])]
    public static bool ClosestInventoryWithStockMultiAccessPrefix(
        DistrictInventoryPicker __instance,
        Accessible start,
        string goodId,
        Predicate<Inventory> inventoryFilter,
        ref Inventory __result)
    {
        if (!start || !ShouldReversePathFromStart(start))
        {
            return true;
        }

        // Path FROM warehouse (finished / may have cache) TO site accesses (no cache needed at end).
        Inventory? best = null;
        var bestDistance = float.MaxValue;

        var stocks = __instance._districtInventoryRegistry.ActiveInventoriesWithStock(goodId);
        foreach (var inventory in stocks)
        {
            if (!inventoryFilter(inventory))
            {
                continue;
            }

            var warehouseAccessible = inventory.GetEnabledComponent<Accessible>();
            if (!warehouseAccessible)
            {
                continue;
            }

            if (TryDistanceWarehouseToSite(warehouseAccessible, start, out var distance)
                && distance < bestDistance)
            {
                best = inventory;
                bestDistance = distance;
            }
        }

        __result = best!;
        return false;
    }

    /// <summary>
    /// Reverse when vanilla would crash: multi-access, or extra-hauler dest without flow-field cache.
    /// Finished workshops keep vanilla start→warehouse pathing.
    /// Do not call <c>GetComponent&lt;Inventory&gt;</c> here — mills/workshops have multiple inventories and throw.
    /// </summary>
    static bool ShouldReversePathFromStart(Accessible start)
    {
        if (!start.HasSingleAccess)
        {
            return true;
        }

        return ExtraHaulerTargetService.Instance?.IsRegisteredAccessible(start) == true;
    }

    /// <summary>
    /// Warehouse → site distance. Cached road first; instant road if entrance has no cache;
    /// road-to-terrain for unfinished sites off the road mesh.
    /// </summary>
    static bool TryDistanceWarehouseToSite(Accessible warehouse, Accessible site, out float distance)
    {
        if (TryFindRoadPath(warehouse, site, out distance))
        {
            return true;
        }

        if (TryFindInstantRoadPath(warehouse, site, out distance))
        {
            return true;
        }

        if (TryFindRoadToTerrainPath(warehouse, site, out distance))
        {
            return true;
        }

        distance = 0f;
        return false;
    }

    static bool TryFindRoadPath(Accessible from, Accessible to, out float distance)
    {
        try
        {
            return from.FindRoadPath(to, out distance);
        }
        catch (InvalidOperationException)
        {
            // No cached road flow field at `from` (stockpile/entrance not caching).
            distance = 0f;
            return false;
        }
    }

    static bool TryFindInstantRoadPath(Accessible from, Accessible to, out float distance)
    {
        try
        {
            return from.FindInstantRoadPath(to, out distance);
        }
        catch (InvalidOperationException)
        {
            distance = 0f;
            return false;
        }
    }

    static bool TryFindRoadToTerrainPath(Accessible from, Accessible to, out float distance)
    {
        try
        {
            return from.FindRoadToTerrainPath(to, out _, out distance);
        }
        catch (InvalidOperationException)
        {
            distance = 0f;
            return false;
        }
    }
}
