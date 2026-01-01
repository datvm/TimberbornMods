namespace WarningsBeGone.Patches;

[HarmonyPatch(typeof(StatusAggregator))]
public class StatusAggregatorPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(StatusAggregator.IsVisible))]
    public static bool AddHidingCheck(StatusInstance statusInstance, ref bool __result)
    {
        if (!StatusHidingService.HideCornerWarnings ||
            !statusInstance.StatusSubject.GetComponent<StatusHidingComponent>().ShouldHide(statusInstance.StatusDescription))
        {
            return true;
        }

        __result = false;
        return false;
    }

}
