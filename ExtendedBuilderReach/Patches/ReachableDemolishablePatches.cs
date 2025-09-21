namespace ExtendedBuilderReach.Patches;

[HarmonyPatch(typeof(ReachableDemolishable))]
public static class ReachableDemolishablePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(ReachableDemolishable.Start))]
    public static void ReplaceAccessible(ReachableDemolishable __instance)
    {
        if (!MSettings.ExtendDemolishValue) { return; }

        var accessible = __instance.GetComponentFast<ExtendedDemolishableAccessible>();
        if (!accessible) { return; }

        __instance._accessible = accessible.Accessible;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AccessibleDemolishableReacher), nameof(AccessibleDemolishableReacher.PostInitializeEntity))]
    public static bool PickAccessible(AccessibleDemolishableReacher __instance)
    {
        if (!MSettings.ExtendDemolishValue) { return true; }

        var accessible = __instance.GetComponentFast<ExtendedDemolishableAccessible>();
        if (!accessible) { return true; }

        __instance._destination = new AccessibleDestination(accessible.Accessible);
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DemolishBehavior), nameof(DemolishBehavior.Decide))]
    public static bool ChangeDecideToWalk(BehaviorAgent agent, ref Decision __result)
    {
        if (!MSettings.ExtendDemolishValue) { return true; }

        var demolisher = agent.GetComponentFast<Demolisher>();
        if (!demolisher.HasReservedDemolishable)
        {
            __result = Decision.ReleaseNow();
            return false;
        }

        if (!demolisher.ReservedDemolishable.CanBeDemolished)
        {
            __result = DemolishBehavior.UnreserveDemolishable(demolisher);
            return false;
        }

        var demolishable = demolisher.Demolishable;
        var reacher = demolishable.GetComponentFast<ExtendedDemolishableAccessible>();        
        if (!reacher) // Not Demolish
        {
            return true;
        }

        var executor = agent.GetComponentFast<WalkToAccessibleExecutor>();

        var target = reacher.Accessible;
        if (!target)
        {
            // Should not happen
            Debug.LogWarning("This branch should not be running. Handling over back to the original code");
            return true;
        } 

        __result = executor.Launch(target) switch
        {
            ExecutorStatus.Success => DemolishBehavior.Demolish(agent),
            ExecutorStatus.Failure => DemolishBehavior.UnreserveDemolishable(agent.GetComponentFast<Demolisher>()),
            ExecutorStatus.Running => Decision.ReturnWhenFinished(executor),
            _ => throw new ArgumentOutOfRangeException(),
        };
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Accessible), nameof(Accessible.ValidAccessible), MethodType.Getter)]
    public static void PatchDisabledSpecialAccessible(Accessible __instance, ref bool __result)
    {
        if (__result || __instance.ComponentName != ExtendedDemolishableAccessible.AccessibleComponentName) { return; }
        __result = __instance.IsValid();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Accessible), nameof(Accessible.UnblockedSingleAccess), MethodType.Getter)]
    public static void PatchDisabledUnblockedSingleAccess(Accessible __instance, ref Vector3? __result)
    {
        if (__result != null
            || __instance.ComponentName != ExtendedDemolishableAccessible.AccessibleComponentName
            || __instance.IsBlocked) { return; }

        __result = __instance.Accesses.Single();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Accessible), nameof(Accessible.UnblockedSingleAccessInstant), MethodType.Getter)]
    public static void PatchDisabledUnblockedSingleAccessInstant(Accessible __instance, ref Vector3? __result)
    {
        if (__result != null
            || __instance.ComponentName != ExtendedDemolishableAccessible.AccessibleComponentName
            || __instance.IsBlockedInstant) { return; }
        __result = __instance.Accesses.Single();
    }


}
