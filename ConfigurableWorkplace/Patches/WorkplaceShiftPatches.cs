namespace ConfigurableWorkplace.Patches;

[HarmonyPatch]
public static class WorkplaceShiftPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(WorkplaceWorkingHours), nameof(WorkplaceWorkingHours.AreWorkingHours), MethodType.Getter)]
    public static bool CheckForCustomHours(WorkplaceWorkingHours __instance, ref bool __result)
    {
        if (!MSettings.BuildingShift || __instance.IgnoreWorkingHours) { return true; }

        var comp = __instance.GetComponentFast<WorkplaceShiftComponent>();
        if (!comp || !comp.EnableCustomShift) { return true; }

        __result = comp.AreWorkingHours;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WorkerWorkingHours), nameof(WorkerWorkingHours.AreWorkingHours), MethodType.Getter)]
    public static bool CheckForCustomHours(WorkerWorkingHours __instance, ref bool __result)
    {
        if (!MSettings.BuildingShift || __instance._ignoreWorkingHours) { return true; }

        var comp = __instance.GetComponentFast<Worker>();
        __result = comp.Workplace.GetComponentFast<WorkplaceWorkingHours>().AreWorkingHours;
        return false;
    }

}
