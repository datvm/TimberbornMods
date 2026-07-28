namespace ExtendedBuilderReach.Patches;

/// <summary>
/// When Extend Demolition Range is on, point demolition reachability and walk destinations at the
/// dedicated ExtendedDemolishable Accessible (multi-level). Leaves GoodStack / Building / BlockObject
/// Accessibles and DemolishBehavior alone.
/// </summary>
[HarmonyPatch]
public static class DemolishableAccessiblePatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(ReachableDemolishable), nameof(ReachableDemolishable.InitializeEntity))]
    public static void PatchReachableDemolishable(ReachableDemolishable __instance)
    {
        if (!TryGetExtendedAccessible(__instance, out var accessible)) { return; }

        __instance._accessible = accessible;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(AccessibleDemolishableReacher), nameof(AccessibleDemolishableReacher.PostInitializeEntity))]
    public static void PatchAccessibleDemolishableReacher(AccessibleDemolishableReacher __instance)
    {
        if (!TryGetExtendedAccessible(__instance, out var accessible)) { return; }

        __instance._destination = new AccessibleDestination(accessible);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(UncuttableReacher), nameof(UncuttableReacher.InitializeEntity))]
    public static void PatchUncuttableReacher(UncuttableReacher __instance)
    {
        if (!TryGetExtendedAccessible(__instance, out var accessible)) { return; }

        __instance._destination = new AccessibleDestination(accessible);
    }

    static bool TryGetExtendedAccessible(BaseComponent component, out Accessible accessible)
    {
        accessible = null!;
        if (!MSettings.ExtendDemolishValue) { return false; }

        var extended = component.GetComponent<ExtendedDemolishableAccessible>();
        if (!extended) { return false; }

        accessible = extended.Accessible;
        return accessible;
    }

}
