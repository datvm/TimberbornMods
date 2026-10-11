namespace WarningsBeGone.Patches;

[HarmonyPatch(typeof(StatusSubject))]
public static class StatusSubjectPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(StatusSubject.UpdateStatus), new[] { typeof(StatusInstance), typeof(bool) })]
    public static void FilterStatus(StatusSubject __instance, StatusInstance statusInstance, bool isActive)
        => __instance.GetComponent<StatusHidingComponent>()?.OnStatusUpdated(statusInstance, isActive);

}
