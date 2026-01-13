namespace ModdableWeathers.Patches;

[HarmonyPatch]
public static class LandMoisturePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MoistureCalculationTask), nameof(MoistureCalculationTask.CalculateMoistureForCell))]
    public static bool SetMoistureIfRaining(ref float __result)
    {
        if (!LandMoistureService.ShouldMoisturize) { return true; }

        __result = 16f;
        return false;
    }

}
