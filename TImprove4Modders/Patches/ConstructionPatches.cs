global using Timberborn.Buildings;
global using Timberborn.BuildingTools;

namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class ConstructionPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BuildingPlacer), nameof(BuildingPlacer.ShouldBePlacedFinished))]
    public static bool PatchSwapBuildingFinished(BuildingSpec buildingSpec, BuildingPlacer __instance, ref bool __result)
    {
        if (!DevModeService.IsDevModeOn
            || !MSettings.SwapBuildFinishedModifier) { return true; }

        __result = !__instance._inputService.IsKeyHeld(BuildingPlacer.PlaceFinishedKey) || buildingSpec.PlaceFinished;
        return false;
    }

}
