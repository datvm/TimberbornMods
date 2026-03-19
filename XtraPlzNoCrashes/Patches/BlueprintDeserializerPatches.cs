namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch]
public static class BlueprintDeserializerPatches
{

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        var types = typeof(BlueprintDeserializer).GetNestedTypes(AccessTools.all)
            .First(t => t.Name.Contains($"<{nameof(BlueprintDeserializer.DeserializeSpecs)}>"));

        return types.Method("MoveNext");
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        var replacingMethod = typeof(Debug).Method(nameof(Debug.LogWarning), [typeof(object)]);

        var list = instructions.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            var instruction = list[i];
            if (instruction.opcode == OpCodes.Throw)
            {
                found = true;

                // Do not create the exception                
                list[i] = new(OpCodes.Call, replacingMethod);
                list.RemoveAt(i - 1);
            }
        }

        if (!found)
        {
            throw new InvalidOperationException("Failed to find the throw instruction in the BlueprintDeserializer method. The patch may be outdated.");
        }

        return list;
    }

}
