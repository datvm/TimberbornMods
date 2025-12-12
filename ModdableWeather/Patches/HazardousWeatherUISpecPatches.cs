using ModdableWeather.HazardousTimer;

namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(HazardousWeatherUISpec))]
public static class HazardousWeatherUISpecPatches
{

    [HarmonyPatch(nameof(HazardousWeatherUISpec.ApproachingNotificationDays), MethodType.Getter)]
    public static bool Modify(ref int __result)
    {
        if (ApproachingTimerModifierService.Instance is null) { return true; }

        __result = ApproachingTimerModifierService.Instance.ApproachingNotificationDays;
        return __result == -1;
    }

}
