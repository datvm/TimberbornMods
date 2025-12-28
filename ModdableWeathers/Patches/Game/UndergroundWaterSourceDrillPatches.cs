namespace ModdableWeathers.Patches.Game;

[HarmonyPatch(typeof(UndergroundWaterSourceDrill))]
public class UndergroundWaterSourceDrillPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(UndergroundWaterSourceDrill.OnEnterFinishedState))]
    public static void BlockIfHazardous(UndergroundWaterSourceDrill __instance)
    {
        if (__instance._isBlocking) { return; }

        if (__instance._hazardousWeatherObserver._weatherService.IsHazardousWeather)
        {
            __instance.Block();
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(UndergroundWaterSourceDrill.GetStrengthModifier))]
    public static bool StopWaterIfBlockedOrBadwater(UndergroundWaterSourceDrill __instance, ref float __result)
    {
        if (__instance._isBlocking || __instance._underlyingWaterSource.WaterSource.Contamination > 0)
        {
            __result = 0f;
            return false;
        }

        return true;
    }

}
