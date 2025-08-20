using System.Reflection.Emit;

namespace HangingOverhang.Patches;

[HarmonyPatch(typeof(TerrainPhysicsPostLoader))]
public static class TerrainPhysicsPostLoaderPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(TerrainPhysicsPostLoader.Enqueue), [typeof(TerrainPhysicsPostLoader.Candidate), typeof(Block)])]
    public static IEnumerable<CodeInstruction> EnqueueTranspiler(IEnumerable<CodeInstruction> instructions) => Add1ToDistance(instructions);

    [HarmonyTranspiler, HarmonyPatch(nameof(TerrainPhysicsPostLoader.ValidateTerrain))]
    public static IEnumerable<CodeInstruction> ValidateTerrainTranspiler(IEnumerable<CodeInstruction> instructions) => Add1ToDistance(instructions);

    static IEnumerable<CodeInstruction> Add1ToDistance(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var i in instructions)
        {
            if (i.opcode == OpCodes.Bge_S)
            {
                found = true;
                yield return new(OpCodes.Ldc_I4_1);
                yield return new(OpCodes.Add);
            }
            yield return i;
        }

        if (!found)
        {
            throw new InvalidOperationException("Could not find the expected instruction in TerrainPhysicsPostLoader.Enqueue transpiler.");
        }
    }

}
