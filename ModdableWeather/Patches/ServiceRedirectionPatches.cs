namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(WeatherService))]
public static class ServiceRedirectionPatches
{

    [HarmonyGetterPatch(nameof(WeatherService.HazardousWeatherDuration))]
    public static bool GetHazardousWeatherDuration(ref int __result)
    {
        __result = ModdableWeatherService.Instance.HazardousWeatherDuration;
        return false;
    }

    [HarmonyGetterPatch(nameof(WeatherService.TemperateWeatherDuration))]
    public static bool GetTemperateWeatherDuration(ref int __result)
    {
        __result = ModdableWeatherService.Instance.TemperateWeatherDuration;
        return false;
    }

    [HarmonyGetterPatch(nameof(WeatherService.HazardousWeatherStartCycleDay))]
    public static bool GetHazardousWeatherStartCycleDay(ref int __result)
    {
        __result = ModdableWeatherService.Instance.HazardousWeatherStartCycleDay;
        return false;
    }

    [HarmonyGetterPatch(nameof(WeatherService.CycleLengthInDays))]
    public static bool GetCycleLengthInDays(ref int __result)
    {
        __result = ModdableWeatherService.Instance.CycleLengthInDays;
        return false;
    }

    [HarmonyGetterPatch(nameof(WeatherService.IsHazardousWeather))]
    public static bool GetIsHazardousWeather(ref bool __result)
    {
        __result = ModdableWeatherService.Instance.IsHazardousWeather;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(WeatherService.Load))]
    public static bool IgnoreLoad() => false;

}
