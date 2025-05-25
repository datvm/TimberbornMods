namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(HazardousWeatherUIHelper))]
public static class HazardousWeatherUIHelperPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(HazardousWeatherUIHelper.UpdateCurrentUISpecification))]
    public static bool UpdateCurrentUISpecificationPrefix(HazardousWeatherUIHelper __instance)
    {
        var weather = (IModdedHazardousWeather) __instance._hazardousWeatherService.CurrentCycleHazardousWeather;
        __instance._currentUISpecification = weather.Spec;

        return false;
    }

}
