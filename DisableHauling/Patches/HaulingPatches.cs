namespace DisableHauling.Patches;

[HarmonyPatch]
public static class HaulingPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(HaulCandidate), nameof(HaulCandidate.GetWeightedBehaviors))]
    public static bool DisableHauling(HaulCandidate __instance) => !IsHaulingDisabled(__instance);

    [HarmonyPrefix, HarmonyPatch(typeof(CarrierInventoryFinder), nameof(CarrierInventoryFinder.TryCarryFromAnyInventoryInternal))]
    public static bool DisableCarryFromAnyInventory(Inventory receivingInventory, ref Predicate<Inventory> inventoryFilter, ref bool __result)
    {
        if (IsHaulingDisabled(receivingInventory))
        {
            __result = false;
            return false;
        }

        inventoryFilter = IgnoreDisabledInventories(inventoryFilter);
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CarrierInventoryFinder), nameof(CarrierInventoryFinder.TryCarryToAnyInventory))]
    public static bool DisableCarryToAnyInventory(Inventory givingInventory, ref Predicate<Inventory> inventoryFilter, ref bool __result)
    {
        if (IsHaulingDisabled(givingInventory))
        {
            __result = false;
            return false;
        }

        inventoryFilter = IgnoreDisabledInventories(inventoryFilter);
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(EmptyingStarter), nameof(EmptyingStarter.StartEmptying), [typeof(Inventory), typeof(bool)])]
    public static bool DisableStartEmptying(Inventory inventory, ref bool __result)
    {
        if (IsHaulingDisabled(inventory))
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(EmptyingStarter), nameof(EmptyingStarter.StartEmptyingUnwantedStock))]
    public static bool DisableStartEmptyingUnwantedStock(Inventory inventory, ref bool __result)
    {
        if (IsHaulingDisabled(inventory))
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CarrierInventoryFinder), nameof(CarrierInventoryFinder.GetClosestInventoryWithCapacity))]
    public static void DisableClosestInventoryWithCapacity(ref Inventory __result)
    {
        if (IsHaulingDisabled(__result))
        {
            __result = null!;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DistrictInventoryPicker), nameof(DistrictInventoryPicker.ClosestInventoryWithStock), [typeof(Vector3), typeof(string), typeof(Accessible)])]
    public static bool DisableClosestInventoryWithStock(DistrictInventoryPicker __instance, Vector3 start, string goodId, Accessible accessibleReachableFromInventory, ref Inventory __result)
    {
        var inventories = __instance._districtInventoryRegistry.ActiveInventoriesWithStock(goodId);
        Inventory result = null!;
        var closestDistance = float.MaxValue;

        foreach (var inventory in inventories)
        {
            if (IsHaulingDisabled(inventory)) { continue; }

            var accessible = inventory.GetEnabledComponent<Accessible>();
            if (accessible.IsReachableByRoad(accessibleReachableFromInventory)
                && accessible.FindRoadPath(start, out var distance)
                && distance < closestDistance)
            {
                result = inventory;
                closestDistance = distance;
            }
        }

        __result = result;
        return false;
    }

    static Predicate<Inventory> IgnoreDisabledInventories(Predicate<Inventory> inventoryFilter) =>
        inventory => inventoryFilter(inventory) && !IsHaulingDisabled(inventory);

    static bool IsHaulingDisabled(BaseComponent component) =>
        component && component.GetComponent<DisableHaulingComponent>()?.DisableHauling == true;

}
