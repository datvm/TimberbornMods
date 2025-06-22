namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class ConstructionKeySwapPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BuildingPlacer), nameof(BuildingPlacer.ShouldBePlacedFinished))]
    public static bool PatchSwapBuildingFinished(BuildingSpec buildingSpec, BuildingPlacer __instance, ref bool __result)
    {
        if (!DevModeService.IsDevModeOn
            || !MSettings.SwapBuildFinishedModifier) { return true; }

        __result = !__instance._inputService.IsKeyHeld(BuildingPlacer.PlaceFinishedKey) || buildingSpec.PlaceFinished;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BlockObjectTool), nameof(BlockObjectTool.ActionCallback))]
    public static void PatchSwapBuildingUnlock(BlockObjectTool __instance)
    {
        if (!DevModeService.IsDevModeOn
            || !MSettings.SwapBuildFinishedModifier
            || __instance.Locker is not BuildingToolLocker locker
            || __instance._inputService.IsKeyHeld(BuildingPlacer.PlaceFinishedKey)) { return; }

        locker.UnlockIgnoringScienceCost(
            BuildingToolLocker.GetBuildingFromToolUnsafe(__instance),
            TimberUiUtils.DoNothing);
        __instance._toolUnlockingService.OnSuccessfulUnlock(__instance, TimberUiUtils.DoNothing);
    }

}
