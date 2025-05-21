namespace ConfigurableBuildingRange.Patches;

[HarmonyPatch]
public static class RangePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(NavigationDistance), nameof(NavigationDistance.ResourceBuildings), MethodType.Getter)]
    public static bool PatchResourceBuildingsRange(ref float __result)
    {
        __result = MSettings.ResourceBuildingsRangeValue;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(NavigationDistance), nameof(NavigationDistance.DistrictTerrain), MethodType.Getter)]
    public static bool PatchDistrictTerrainRange(ref int __result)
    {
        __result = MSettings.DistrictTerrainRangeValue;
        return false;
    }
}
