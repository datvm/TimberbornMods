namespace MapResizer.Patches;

[HarmonyPatch]
public static class SavePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Ticker), nameof(Ticker.FinishFullTick))]
    public static bool SkipFullTick() => !MapResizeService.PerformingResize;

    [HarmonyPrefix, HarmonyPatch(typeof(WaterEvaporationMap), nameof(WaterEvaporationMap.Save))]
    public static bool SkipWaterEvaporationMapSave() => !MapResizeService.PerformingResize;

    [HarmonyPrefix, HarmonyPatch(typeof(LevelVisibilityService), nameof(LevelVisibilityService.SetLevelsWithAnythingHidable))]
    public static bool SkipSetLevelsWithAnythingHidable() => !MapResizeService.PerformingResize;

}
