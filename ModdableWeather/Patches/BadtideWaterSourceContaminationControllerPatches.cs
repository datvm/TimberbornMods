namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(BadtideWaterSourceContaminationController))]
public static class BadtideWaterSourceContaminationControllerPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(BadtideWaterSourceContaminationController.IsBadtideWeather), MethodType.Getter)]
    public static bool ModdedIsBadtideWeather(ref bool __result)
    {
        __result = ModdableWeatherService.Instance.CurrentWeather is GameBadtideWeather;

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(BadtideWaterSourceContaminationController.OnHazardousWeatherStarted))]
    public static bool ModdedOnHazardousWeatherStarted(HazardousWeatherStartedEvent hazardousWeatherStartedEvent, BadtideWaterSourceContaminationController __instance)
    {
        if (hazardousWeatherStartedEvent.HazardousWeather.IsBadtide())
        {
            __instance.enabled = true;
        }

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(BadtideWaterSourceContaminationController.OnHazardousWeatherEnded))]
    public static bool ModdedOnHazardousWeatherEnded(HazardousWeatherEndedEvent hazardousWeatherEndedEvent, BadtideWaterSourceContaminationController __instance)
    {
        if (hazardousWeatherEndedEvent.HazardousWeather.IsBadtide())
        {
            __instance._waterSourceContamination.ResetContamination();
            __instance.enabled = false;
        }
        return false;
    }

}
