namespace WarningsBeGone.Patches;

[HarmonyPatch(typeof(StatusAggregator))]
public class StatusAggregatorPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(StatusAggregator.IsVisible))]
    public static bool AddHidingCheck(StatusInstance statusInstance, ref bool __result)
    {
        if (!StatusHidingService.HideCornerWarnings) { return true; }

        var description = statusInstance.StatusDescription;
        if (!StatusHidingService.IsDescriptionHiddenAnywhere(description)) { return true; }
        if (StatusHidingService.IsHiddenGlobally(description))
        {
            __result = false;
            return false;
        }

        var subject = statusInstance.StatusSubject;
        if (!subject) { return true; }

        var comp = subject.GetComponent<StatusHidingComponent>();
        if (!comp || !comp.ShouldHide(description)) { return true; }

        __result = false;
        return false;
    }

}
