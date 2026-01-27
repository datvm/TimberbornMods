using System.Reflection.Emit;

namespace ConfigurableExplosives.Patches;

[HarmonyPatch]
public static class DynamiteDetonationPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Dynamite), nameof(Dynamite.Tick))]
    public static bool PreTick() => false;

    [HarmonyPrefix, HarmonyPatch(typeof(Dynamite), nameof(Dynamite.Depth), MethodType.Getter)]
    public static bool PatchDepth(Dynamite __instance, ref int __result)
    {
        var comp = __instance.GetComponent<ConfigurableDynamiteComponent>();
        if (!comp) { return true; }

        __result = comp.DetonationDepth;
        return false;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(Dynamite), nameof(Dynamite.TriggerNeighbors))]
    public static IEnumerable<CodeInstruction> PatchTriggerNeighbors(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        foreach (var i in instructions)
        {
            if (i.opcode == OpCodes.Ldsfld && i.operand as FieldInfo == typeof(Deltas).Field(nameof(Deltas.Neighbors4Vector3Int)))
            {
                found = true;
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Call, typeof(DynamiteDetonationPatches).Method(nameof(GetNeighbors)));
            }
            else
            {
                yield return i;
            }
        }

        if (!found)
        {
            throw new InvalidOperationException("Failed to apply Dynamite.TriggerNeighbors transpiler patch.");
        }
    }

    static Vector3Int[] GetNeighbors(Dynamite dynamite)
    {
        var comp = dynamite.GetComponent<ConfigurableDynamiteComponent>();
        if (!comp) { return Deltas.Neighbors4Vector3Int; }

        var rad = comp.TriggerRadius;

        if (rad < 1) { return []; }

        List<Vector3Int> neighbors = [];

        for (int x = -rad; x <= rad; x++)
        {
            for (int y = -rad; y <= rad; y++)
            {
                for (int z = -rad; z <= rad; z++)
                {
                    var coord = new Vector3Int(x, y, z);
                    if (coord == Vector3Int.zero) { continue; }

                    neighbors.Add(coord);
                }
            }
        }

        return [.. neighbors];
    }

}
