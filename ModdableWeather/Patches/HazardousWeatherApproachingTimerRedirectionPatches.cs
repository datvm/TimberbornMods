namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(HazardousWeatherApproachingTimer))]
public static class HazardousWeatherApproachingTimerRedirectionPatches
{
    [HarmonyGetterPatch(nameof(HazardousWeatherApproachingTimer.TooCloseToNotify))]
    public static bool GetTooCloseToNotify(ref bool __result)
    {
        __result = ModdableHazardousWeatherApproachingTimer.Instance.TooCloseToNotify;
        return false;
    }

    [HarmonyPatch(nameof(HazardousWeatherApproachingTimer.GetProgress))]
    [HarmonyPrefix]
    public static bool GetProgress(ref float __result)
    {
        __result = ModdableHazardousWeatherApproachingTimer.Instance.GetProgress();
        return false;
    }

    [HarmonyPatch(nameof(HazardousWeatherApproachingTimer.OnCycleDayStarted))]
    [HarmonyPrefix]
    public static bool OnCycleDayStarted(CycleDayStartedEvent cycleDayStartedEvent)
    {
        ModdableHazardousWeatherApproachingTimer.Instance.OnCycleDayStarted(cycleDayStartedEvent);
        return false;
    }
}
