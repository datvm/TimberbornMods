using ModdableWeather.Weathers;

namespace ModdableWeather.Patches.Weathers;

[HarmonyPatch]
public static class RainWeatherPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SoilMoistureSimulator), nameof(MoistureCalculationJob.CalculateMoistureForCell))]
    public static bool SetMoistureIfRaining(ref float __result)
    {
        if (!RainWeather.IsRaining) { return true; }

        __result = 16f;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationService), nameof(SoilContaminationService.Contamination))]
    public static bool SetContaminationIfRaining(ref float __result)
    {
        if (!RainWeather.IsRaining) { return true; }

        __result = 0;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationService), nameof(SoilContaminationService.SoilIsContaminated))]
    public static bool SetContaminationIfRaining(ref bool __result)
    {
        if (!RainWeather.IsRaining) { return true; }

        __result = false;
        return false;
    }

}
