namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(HazardousWeatherService))]
public static class ModdableHazardousWeatherServiceRedirectionPatches
{
    [HarmonyGetterPatch(nameof(HazardousWeatherService.HazardousWeatherDuration))]
    public static bool GetHazardousWeatherDuration(ref int __result)
    {
        __result = ModdableHazardousWeatherService.Instance.HazardousWeatherDuration;
        return false;
    }

    [HarmonyGetterPatch(nameof(HazardousWeatherService.DurationInDays))]
    public static bool GetDurationInDays(ref int __result)
    {
        __result = ModdableHazardousWeatherService.Instance.DurationInDays;
        return false;
    }

    [HarmonyGetterPatch(nameof(HazardousWeatherService.CurrentCycleHazardousWeather))]
    public static bool GetCurrentCycleHazardousWeather(ref IHazardousWeather __result)
    {
        __result = ModdableHazardousWeatherService.Instance.CurrentCycleHazardousWeather;
        return false;
    }
}
