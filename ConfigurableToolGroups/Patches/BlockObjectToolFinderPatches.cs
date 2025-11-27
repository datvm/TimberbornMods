using System.Reflection.Emit;

namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(BlockObjectToolFinder))]
public static class BlockObjectToolFinderPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(BlockObjectToolFinder.TryFindTool))]
    public static IEnumerable<CodeInstruction> AllowMultiple(IEnumerable<CodeInstruction> instructions)
    {

        foreach (var ins in instructions)
        {
            if (ins.opcode == OpCodes.Call && ins.operand is MethodInfo method && method.Name == nameof(Enumerable.SingleOrDefault))
            {
                var parameters = method.GetParameters();

                var replacingMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == nameof(Enumerable.FirstOrDefault) && m.GetParameters().Length == parameters.Length);

                replacingMethod = replacingMethod.MakeGenericMethod(method.GetGenericArguments());
                yield return new CodeInstruction(OpCodes.Call, replacingMethod);
            }
            else
            {
                yield return ins;
            }
        }
    }

}
