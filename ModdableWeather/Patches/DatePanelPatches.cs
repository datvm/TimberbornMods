namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(DatePanel))]
public static class DatePanelPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(DatePanel.UpdatePanel))]
    public static bool PatchUpdatePanel()
    {
        ModdableDatePanel.Instance.NewUpdatePanel();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(DatePanel.UpdateText))]
    public static bool PatchUpdateText()
    {
        ModdableDatePanel.Instance.NewUpdateText(ModdableWeatherService.Instance.CurrentWeather.Spec);
        return false;
    }

}
