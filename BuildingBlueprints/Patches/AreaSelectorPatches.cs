using System.Reflection.Emit;

namespace BuildingBlueprints.Patches;

[HarmonyPatch(typeof(AreaSelector))]
public static class AreaSelectorPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(AreaSelector.ClampSelectionEnd))]
    public static IEnumerable<CodeInstruction> ClampSelectionEndTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        foreach (var i in instructions)
        {
            if (!found && i.opcode == OpCodes.Ldc_I4_S && (sbyte)i.operand == 30)
            {
                found = true;
                yield return CodeInstruction.Call(() => GetMaxAreaSize());
            }
            else
            {
                yield return i;
            }
        }

        if (!found)
        {
            throw new Exception("Failed to find the instruction to patch in AreaSelector.ClampSelectionEnd");
        }
    }

    public static int GetMaxAreaSize() => CreateBuildingBlueprintTool.IsSelecting ? int.MaxValue : 30;

}
