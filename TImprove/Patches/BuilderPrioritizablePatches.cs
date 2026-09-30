using System.Reflection.Emit;

namespace TImprove.Patches;

[HarmonyPatch(typeof(BuilderPrioritizable))]
public static class BuilderPrioritizablePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(BuilderPrioritizable.SetPriority))]
    public static void OnPrioritySet(Priority priority)
    {
        BuildingPrioritizableService.Instance?.LastSetPriority = priority;
    }

    [HarmonyTranspiler, HarmonyPatch(nameof(BuilderPrioritizable.Save))]
    public static IEnumerable<CodeInstruction> RemoveSaveSkipping(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        foreach (var i in instructions)
        {
            if (!found && i.opcode == OpCodes.Beq_S)
            {
                found = true;
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Pop);
            }
            else
            {
                yield return i;
            }
        }

        if (!found)
        {
            throw new Exception("Failed to find the instruction to remove in BuilderPrioritizable.Save");
        }
    }

}

[HarmonyPatch]
public static class BuilderPrioritizableConstructorPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod() => typeof(BuilderPrioritizable).Constructor();

    [HarmonyPostfix]
    public static void SetDefaultPriority(BuilderPrioritizable __instance)
    {
        var ins = BuildingPrioritizableService.Instance;
        if (ins is null) { return; }

        __instance.Priority = ins.DefaultBuildingPriority;
    }
}