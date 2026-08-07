namespace ModdableTimberborn.HaulingSystem.Patches;

/// <summary>
/// Vanilla <see cref="DistrictInventoryPicker.ClosestInventoryWithStock(Accessible, string, Predicate{Inventory})"/>
/// paths from <c>start</c> via <see cref="Accessible.UnblockedSingleAccess"/>, which requires exactly
/// one access point. Construction sites use multi-access <see cref="ConstructionSiteAccessible"/>,
/// so we reverse the path direction (warehouse single-access → site multi-access), matching how
/// builders find stock near a construction site.
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
        if (!start || start.HasSingleAccess)
        {
            return true;
        }

        // Multi-access start (e.g. construction site): path FROM warehouse TO site accesses.
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

            // Road warehouse → any site access (end supports multi-access).
            if (warehouseAccessible.FindRoadPath(start, out var roadDistance)
                && roadDistance < bestDistance)
            {
                best = inventory;
                bestDistance = roadDistance;
                continue;
            }

            // Road → terrain to site access (typical for unfinished construction).
            if (warehouseAccessible.FindRoadToTerrainPath(start, out _, out var terrainDistance)
                && terrainDistance < bestDistance)
            {
                best = inventory;
                bestDistance = terrainDistance;
            }
        }

        __result = best!;
        return false;
    }
}
