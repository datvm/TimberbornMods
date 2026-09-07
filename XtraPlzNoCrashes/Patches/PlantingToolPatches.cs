namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch]
public static class PlantingToolPatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(PlantingToolButtonFactory), nameof(PlantingToolButtonFactory.GetPlanterBuildingName))]
    public static IEnumerable<CodeInstruction> AllowMultiples(IEnumerable<CodeInstruction> instructions)
    {
        var singleMet = typeof(Enumerable).GetMethods(AccessTools.all)
            .First(m => m.Name == nameof(Enumerable.Single) && m.GetParameters().Length == 2);

        var found = false;
        foreach (var ins in instructions)
        {
            if (ins.opcode == OpCodes.Call && ins.operand is MethodInfo m && m.GetGenericMethodDefinition() == singleMet)
            {
                found = true;

                var type = m.GetGenericArguments()[0];
                var first = typeof(Enumerable).GetMethods(AccessTools.all)
                    .First(m => m.Name == nameof(Enumerable.First) && m.GetParameters().Length == 2)
                    .MakeGenericMethod(type);
                yield return new(OpCodes.Call, first);
            }
            else
            {
                yield return ins;
            }
        }

        if (!found)
        {
            throw new InvalidOperationException("Failed to find the call to Enumerable.Single in PlantingToolButtonFactory.GetPlanterBuildingName");
        }
    }



}
