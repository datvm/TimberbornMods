namespace BeavlineLogistics.Patches;

[HarmonyPatch(typeof(UnreachableBuildingStatus))]
public static class UnreachableBuildingStatusPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(UnreachableBuildingStatus.UpdateStatus))]
    public static bool IgnoreWarning(UnreachableBuildingStatus __instance)
    {
        var comp = __instance.GetComponentFast<BeavlineBalancerComponent>();
        if (comp && comp.DisableEntranceWarning)
        {
            __instance._unreachableStatus.Deactivate();
            return false;
        }

        return true;
    }

}
