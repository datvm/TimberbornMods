namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class PathRangePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BuildingRangeDrawer), nameof(BuildingRangeDrawer.DrawRange))]
    public static bool DisableBuildingDrawRange(BuildingRangeDrawer __instance)
    {
        return !MSettings.RemovePathHighlight || __instance.GetComponentFast<DistrictCenter>();
    }

}