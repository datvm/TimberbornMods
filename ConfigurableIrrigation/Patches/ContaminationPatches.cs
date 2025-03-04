using Timberborn.SoilContaminationSystem;

namespace ConfigurableIrrigation.Patches;

[HarmonyPatch]
public static class ContaminationPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationSimulatorSpec), nameof(SoilContaminationSimulatorSpec.MinimumWaterContamination), MethodType.Getter)]
    public static bool PatchMinContamination(ref float __result)
    {
        __result = MSettings.ContaMinimumWaterContamination;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationSimulatorSpec), nameof(SoilContaminationSimulatorSpec.MaxRangeFromSource), MethodType.Getter)]
    public static bool PatchMaxRangeFromSource(ref int __result)
    {
        __result = MSettings.ContaMaxSpread;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationSimulatorSpec), nameof(SoilContaminationSimulatorSpec.VerticalSpreadCostMultiplier), MethodType.Getter)]
    public static bool PatchVerticalSpreadCost(ref float __result)
    {
        __result = MSettings.ContaVerticalSpreadCostMultiplier;
        return false;
    }
}
