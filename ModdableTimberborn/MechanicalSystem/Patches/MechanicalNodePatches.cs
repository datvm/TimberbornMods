namespace ModdableTimberborn.MechanicalSystem.Patches;

[HarmonyPatchCategory(ModdableMechanicalSystemConfig.PatchCategoryName), HarmonyPatch(typeof(MechanicalNode))]
public static class MechanicalNodePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(MechanicalNode.Awake))]
    public static void AwakePostfix(MechanicalNode __instance)
    {
        __instance.PatchAwakePostfix<MechanicalNode, ModdableMechanicalNode>();
    }

    [HarmonyTranspiler, HarmonyPatch(nameof(MechanicalNode.UpdateInput))]
    public static IEnumerable<CodeInstruction> UpdateInputTranspiler(IEnumerable<CodeInstruction> instructions)
        => UpdateInputOutputTranspiler(instructions);

    [HarmonyTranspiler, HarmonyPatch(nameof(MechanicalNode.UpdateOutput))]
    public static IEnumerable<CodeInstruction> UpdateOutputTranspiler(IEnumerable<CodeInstruction> instructions)
        => UpdateInputOutputTranspiler(instructions);

    public static IEnumerable<CodeInstruction> UpdateInputOutputTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        IEnumerable<CodeInstruction> TranspilerWithFoundDelegate(StrongBox<bool> found)
        {
            foreach (var ins in instructions)
            {
                yield return ins;

                if (!found.Value && ins.opcode == OpCodes.Callvirt
                    && (object)ins.operand == typeof(MechanicalGraph).Method(nameof(MechanicalGraph.DeactivateNode)))
                {
                    found.Value = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, typeof(MechanicalNodePatches).Method(nameof(CallUpdate)));
                }
            }
        }

        return instructions.TranspileAndThrowIfNotFound(TranspilerWithFoundDelegate);
    }

    static void CallUpdate(MechanicalNode node)
    {
        var moddable = node.GetComponentFast<ModdableMechanicalNode>();
        moddable.UpdateValues();
    }

}
