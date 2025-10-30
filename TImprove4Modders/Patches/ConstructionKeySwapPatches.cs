using System.Reflection.Emit;

namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class ConstructionKeySwapPatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(BuildingPlacer), nameof(BuildingPlacer.ShouldBePlacedFinished))]
    public static IEnumerable<CodeInstruction> PatchSwapBuildingFinished(IEnumerable<CodeInstruction> instructions)
    {
        var targetField = typeof(BuildingPlacer).Field(nameof(BuildingPlacer._inputService));

        var found = false;
        var skip = 0;
        foreach (var ins in instructions)
        {
            if (skip > 0)
            {
                skip--;
                continue;
            }
            yield return ins;

            if (ins.opcode == OpCodes.Ldfld && ins.operand is FieldInfo f && f == targetField)
            {
                found = true;
                skip = 2;

                yield return CodeInstruction.Call(typeof(ConstructionKeySwapPatches), nameof(PlaceFinishedModifierEnabled));
            }
        }

        if (!found)
        {
            throw new Exception($"Transpiler failed in {nameof(PatchSwapBuildingFinished)}: target field not found.");
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(BuildingToolLocker), nameof(BuildingToolLocker.TryToUnlock))]
    public static IEnumerable<CodeInstruction> PatchSwapInstantUnlock(IEnumerable<CodeInstruction> instructions)
    {
        var targetField = typeof(BuildingToolLocker).Field(nameof(BuildingToolLocker._inputService));
        var found = false;
        var skip = 0;
        foreach (var ins in instructions)
        {
            if (skip > 0)
            {
                skip--;
                continue;
            }
            yield return ins;
            if (ins.opcode == OpCodes.Ldfld && ins.operand is FieldInfo f && f == targetField)
            {
                found = true;
                skip = 2;

                yield return CodeInstruction.Call(typeof(ConstructionKeySwapPatches), nameof(UnlockInstantlyModifierEnabled));
                // Revert it
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                yield return new CodeInstruction(OpCodes.Ceq);
            }

        }

        if (!found)
        {
            throw new Exception($"Transpiler failed in {nameof(PatchSwapInstantUnlock)}: target field not found.");
        }
    }

    static bool IsDevModifierEnabled(string key, InputService inputService)
    {
        if (!DevModeService.IsDevModeOn) { return false; }

        var keyPress = inputService.IsKeyHeld(key);
        return MSettings.SwapBuildFinishedModifier ? !keyPress : keyPress;
    }

    public static bool PlaceFinishedModifierEnabled(InputService inputService) => IsDevModifierEnabled(BuildingPlacer.PlaceFinishedKey, inputService);
    public static bool UnlockInstantlyModifierEnabled(InputService inputService) => IsDevModifierEnabled(BuildingToolLocker.InstantUnlockKey, inputService);

}
