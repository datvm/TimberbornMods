
namespace ModdableWeather.Patches.Weathers;

public static class RainWeatherPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SoilMoistureService), nameof(SoilMoistureService.SoilIsMoist))]
    public static bool SetMoistureIfRaining(ref bool __result)
    {
        if (RainWeather.Instance?.Active != true) { return true; }

        __result = true;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationService), nameof(SoilContaminationService.Contamination))]
    public static bool SetContaminationIfRaining(ref float __result)
    {
        if (RainWeather.Instance?.Active != true) { return true; }

        __result = 0;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationService), nameof(SoilContaminationService.SoilIsContaminated))]
    public static bool SetContaminationIfRaining(ref bool __result)
    {
        if (RainWeather.Instance?.Active != true) { return true; }

        __result = false;
        return false;
    }

}
