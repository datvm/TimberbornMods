namespace WarningsBeGone.Patches;

[HarmonyPatch(typeof(StatusSubject))]
public static class StatusSubjectPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(StatusSubject.ActiveStatuses), MethodType.Getter)]
    public static bool FilterStatuses(StatusSubject __instance, ref ReadOnlyList<StatusInstance> __result)
    {
        if (!__instance) { return true; }

        var comp = __instance.GetComponent<StatusHidingComponent>();
        if (!comp) { return true; }

        __result = comp.GetFilteredActiveStatuses();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(StatusSubject.InPriorityMode), MethodType.Getter)]
    public static bool FilterInPriorityMode(StatusSubject __instance, ref bool __result)
    {
        var comp = __instance.GetComponent<StatusHidingComponent>();
        if (!comp) { return true; }

        __result = comp.IsFilteredInPriorityMode();
        return false;
    }

}
