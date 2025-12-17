using System.Reflection.Emit;

namespace ExtendedOptionalModSupport.Patches;

[HarmonyPatch]
public static class AllowUndeclaredSpecsPatches
{

    public static IEnumerable<MethodBase> TargetMethods()
    {
        var stateMachine = typeof(BlueprintDeserializer).GetNestedTypes(AccessTools.all)
            .FirstOrDefault(t =>
            {
                return t.Name.Contains(nameof(BlueprintDeserializer.DeserializeSpecs))
                    && t.GetCustomAttribute<CompilerGeneratedAttribute>() is not null
                    && typeof(IEnumerator).IsAssignableFrom(t);
            })
            ?? throw new InvalidOperationException($"Cannot find state machine for {nameof(BlueprintDeserializer.DeserializeSpecs)}");

        return [stateMachine.Method(nameof(IEnumerator.MoveNext))];
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            yield return ins.opcode == OpCodes.Throw
                ? CodeInstruction.Call(() => LogWarning)
                : ins;
        }
    }

    static readonly HashSet<string> PrintedTypes = [];
    static void LogWarning(Exception ex)
    {
        if (PrintedTypes.Add(ex.Message))
        {
            var lastSpace = ex.Message.LastIndexOf(' ');
            var type = ex.Message[(lastSpace + 1)..];

            Debug.LogWarning($"Spec type '{type}' does not have a Spec type.");
        }
    }

}
