using System.Reflection.Emit;

namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch(typeof(MaterialRepository))]
public static class MaterialRepositoryPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(MaterialRepository.Load))]
    public static IEnumerable<CodeInstruction> DoNotThrowOnDuplicate(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            yield return instruction.opcode == OpCodes.Throw ? new(OpCodes.Pop) : instruction;
        }
    }
}
