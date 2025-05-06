namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class PathRangePatches
{
    static bool ShouldStillDrawPath;

    [HarmonyPrefix, HarmonyPatch(typeof(BuildingRangeDrawer), nameof(BuildingRangeDrawer.DrawRange))]
    public static bool DisablePathDrawingWithoutRange(BuildingRangeDrawer __instance)
    {
        if (!MSettings.RemovePathHighlight) { return true; }

        ShouldStillDrawPath = __instance._drawRoadSpilledRange;
        return __instance._drawTerrainRange || ShouldStillDrawPath;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PathRangeDrawer), nameof(PathRangeDrawer.DrawRange))]
    public static void MarkPathDraw()
    {
        ShouldStillDrawPath = true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DistrictPathNavRangeDrawer), nameof(DistrictPathNavRangeDrawer.DrawRange))]
    public static bool DisablePathDrawingForRanged()
    {
        if (!MSettings.RemovePathHighlight) { return true; }

        if (ShouldStillDrawPath)
        {
            ShouldStillDrawPath = false;
            return true;
        }

        return false;
    }

}