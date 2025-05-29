namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(WeatherPanel))]
public static class WeatherPanelRedirectionPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(WeatherPanel.UpdatePanel))]
    public static bool RedirectUpdatePanel()
    {
        ModdableWeatherPanel.Instance.UpdatePanel();
        return false;
    }

}
