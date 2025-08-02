namespace SkyblightWeather.Patches;

[HarmonyPatch]
public static class BadrainWeatherPatches
{
    [HarmonyPostfix, HarmonyPatch(typeof(MoistureCalculationJob), nameof(MoistureCalculationJob.GetMoisture), [typeof(int)])]
    public static void LimitIfRaining(ref int __result)
    {
        if (BadrainWeather.IsRaining && __result > BadrainWeather.MaxMoisture)
        {
            __result = BadrainWeather.MaxMoisture;
        }
    }
}
