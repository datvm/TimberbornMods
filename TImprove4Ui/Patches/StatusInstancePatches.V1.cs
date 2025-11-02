namespace TImprove4Ui.Patches;

[HarmonyPatch(typeof(StatusInstance))]
public static class StatusInstancePatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(StatusInstanceFactory), nameof(StatusInstanceFactory.CreateStatus))]
    public static void OnStatusInstanceCreated(StatusSubject statusSubject, StatusInstance __result)
    {
        var tracker = statusSubject.GetComponent<StatusTracker>();
        if (!tracker) { return; }

        tracker.AddStatus(__result);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(StatusInstance.ShowFloatingIcon), MethodType.Getter)]
    public static bool PatchFloatingIcon(StatusInstance __instance, ref bool __result)
    {
        if (PauseStatusIconRegistry.Instance?.ShouldDisable(__instance) == true)
        {
            __result = false;
            return false;
        }

        return true;
    }

}
