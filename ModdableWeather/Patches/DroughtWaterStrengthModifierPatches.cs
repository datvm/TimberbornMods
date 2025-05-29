namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(DroughtWaterStrengthModifier))]
public static class DroughtWaterStrengthModifierPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(DroughtWaterStrengthModifier.IsCycleWithDrought), MethodType.Getter)]
    public static bool ModdedIsCycleWithDrought(ref bool __result)
    {
        __result = ModdableWeatherHistoryProvider.Instance.IsCycleWithDrought;
        return false;
    }


    [HarmonyPrefix, HarmonyPatch(nameof(DroughtWaterStrengthModifier.PreviousCycleHadDrought), MethodType.Getter)]
    public static bool ModdedPreviousCycleHadDrought(ref bool __result)
    {
        __result = ModdableWeatherHistoryProvider.Instance.PreviousCycleHadDrought;
        return false;
    }

}
