namespace BetterWeatherStation.Patches;

[HarmonyPatch(typeof(WeatherStation))]
public static class WeatherStationPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(WeatherStation.Sample))]
    public static bool RedirectSample(WeatherStation __instance) => RedirectToSample(__instance);

    [HarmonyPrefix, HarmonyPatch(nameof(WeatherStation.UpdateOutputState))]
    public static bool RedirectUpdateOutputState(WeatherStation __instance) => RedirectToSample(__instance);

    public static bool RedirectToSample(WeatherStation __instance)
    {
        __instance.GetComponent<BetterWeatherStationComponent>().Sample();
        return false;
    }

}
