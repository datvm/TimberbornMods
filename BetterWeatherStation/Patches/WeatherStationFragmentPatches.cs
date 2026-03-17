namespace BetterWeatherStation.Patches;

[HarmonyPatch(typeof(WeatherStationFragment))]
public static class WeatherStationFragmentPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(WeatherStationFragment.InitializeFragment))]
    public static void HideWeatherSelection(VisualElement __result)
    {
        __result.Q(name: "ModeSection").SetDisplay(false);
    }

}
