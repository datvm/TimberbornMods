namespace WarningsBeGone.Patches;

[HarmonyPatch(typeof(StatusSubject))]
public static class StatusSubjectPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(StatusSubject.ActiveStatuses), MethodType.Getter)]
    public static bool FilterStatuses(StatusSubject __instance, ref ReadOnlyList<StatusInstance> __result)
    {
        __result = __instance.GetComponent<StatusHidingComponent>().GetFilteredActiveStatuses();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(StatusSubject.InPriorityMode), MethodType.Getter)]
    public static bool FilterInPriorityMode(StatusSubject __instance, ref bool __result)
    {
        __result = __instance.GetComponent<StatusHidingComponent>().IsFilteredInPriorityMode();
        return false;
    }

}
