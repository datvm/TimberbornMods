global using System.Reflection.Emit;

namespace ConfigurableIrrigation.Patches;

[HarmonyPatch]
public static class IrrigationPatches
{
    static readonly float DefaultMaxSpread = 16f;

    [HarmonyPrefix, HarmonyPatch(typeof(SoilMoistureSimulatorSpec), nameof(SoilMoistureSimulatorSpec.MinimumWaterContamination), MethodType.Getter)]
    public static bool PatchMinContamination(ref float __result)
    {
        __result = MSettings.MinimumWaterContamination;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilMoistureSimulatorSpec), nameof(SoilMoistureSimulatorSpec.MaximumWaterContamination), MethodType.Getter)]
    public static bool PatchMaxContamination(ref float __result)
    {
        __result = MSettings.MaximumWaterContamination;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilMoistureSimulatorSpec), nameof(SoilMoistureSimulatorSpec.VerticalSpreadCostMultiplier), MethodType.Getter)]
    public static bool PatchVerticalSpreadCost(ref int __result)
    {
        __result = MSettings.VerticalSpreadCostMultiplier;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MoistureCalculationJob), nameof(MoistureCalculationJob.GetInitialMoisturizerRange))]
    public static bool PatchInitialMoisturizerRange(int index3D, ref float __result, MoistureCalculationJob __instance)
    {
        __result = 2 * __instance._clusterSaturation[index3D] * (MSettings.MaxSpread / DefaultMaxSpread);
        return false;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(MoistureCalculationJob), nameof(MoistureCalculationJob.CalculateMoistureForCell))]
    public static IEnumerable<CodeInstruction> PatchMaxSpread(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var i in instructions)
        {
            if (i.opcode == OpCodes.Ldc_R4 && (float?)i.operand == DefaultMaxSpread)
            {
                Debug.Log($"Changing max spread from {i.operand} to {MSettings.MaxSpread}");
                yield return new(OpCodes.Ldc_R4, (float)MSettings.MaxSpread);
            }
            else
            {
                yield return i;
            }
        }
    }

}
