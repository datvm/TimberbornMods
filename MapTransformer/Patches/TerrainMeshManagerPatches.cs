using System.Reflection.Emit;

namespace MapTransformer.Patches;

[HarmonyPatch(typeof(TerrainMeshManager))]
public static class TerrainMeshManagerPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(TerrainMeshManager.InitializeTerrainTiles))]
    public static IEnumerable<CodeInstruction> AddOneLevelToChunk(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        var expectingMethod = typeof(MapSize).PropertyGetter(nameof(MapSize.MaxGameTerrainHeight));

        foreach (var ins in instructions)
        {
            yield return ins;

            if (!found)
            {
                if (ins.opcode == OpCodes.Callvirt && ins.operand is MethodInfo method && method == expectingMethod)
                {
                    found = true;

                    yield return new(OpCodes.Ldc_I4_1);
                    yield return new(OpCodes.Add);
                }
            }
        }

        if (!found)
        {
            throw new InvalidOperationException($"Failed to patch {nameof(TerrainMeshManager)}.{nameof(TerrainMeshManager.InitializeTerrainTiles)}");
        }

    }

}
