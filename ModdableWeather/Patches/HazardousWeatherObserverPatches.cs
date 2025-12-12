namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(HazardousWeatherObserver))]
public static class HazardousWeatherObserverPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(HazardousWeatherObserver.IsBadtideWeather), MethodType.Getter)]
    public static bool PatchIsBadtideWeather(ref bool __result)
    {
        __result = ModdableWeatherService.Instance.CurrentWeather is GameBadtideWeather;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(HazardousWeatherObserver.IsDroughtWeather), MethodType.Getter)]
    public static bool PatchIsDroughtWeather(ref bool __result)
    {
        __result = ModdableWeatherService.Instance.CurrentWeather is GameDroughtWeather;
        return false;
    }


}
