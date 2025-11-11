using System.Reflection.Emit;

namespace ConfigurablePumps.Patches;

[HarmonyPatch]
public static class WaterConversionPatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(WaterGoodToWaterAmountConverter), nameof(WaterGoodToWaterAmountConverter.GetWaterAmount))]
    public  static IEnumerable<CodeInstruction> PatchWaterAmount(IEnumerable<CodeInstruction> instructions)
    {
        var targetField = typeof(WaterGoodToWaterAmountConverter).Field(nameof(WaterGoodToWaterAmountConverter.WaterAmountConversion));
        return RedirectAmount(instructions, targetField);
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(WaterContaminationGoodToWaterContaminationAmountConverter), nameof(WaterContaminationGoodToWaterContaminationAmountConverter.GetWaterContaminationAmount))]
    public static IEnumerable<CodeInstruction> PatchBadWaterAmount(IEnumerable<CodeInstruction> instructions)
    {
        var targetField = typeof(WaterContaminationGoodToWaterContaminationAmountConverter).Field(nameof(WaterContaminationGoodToWaterContaminationAmountConverter.WaterContaminationAmountConversion));
        return RedirectAmount(instructions, targetField);
    }


    public static IEnumerable<CodeInstruction> RedirectAmount(IEnumerable<CodeInstruction> instructions, FieldInfo target)
    {
        foreach (var ins in instructions)
        {
            yield return (object)ins.operand == target && ins.opcode == OpCodes.Ldfld
                ? new(OpCodes.Call, typeof(MSettings).PropertyGetter(nameof(MSettings.WaterConversation)))
                : ins;
        }
    }

}
