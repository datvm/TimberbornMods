using System.Reflection.Emit;

namespace ConfigurableWorkplace.Patches;

[HarmonyPatch]
public static class DesiredWorkersPatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(WorkplaceFragment), nameof(WorkplaceFragment.UpdateButtons))]
    public static IEnumerable<CodeInstruction> AllowZeroWorker(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var i in instructions)
        {
            if (i.opcode == OpCodes.Ldc_I4_1)
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_0); // Change 1 to 0
            }
            else
            {
                yield return i;
            }
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(Workplace), nameof(Workplace.DecreaseDesiredWorkers))]
    public static IEnumerable<CodeInstruction> AllowZeroWorkerDecrease(IEnumerable<CodeInstruction> instructions)
    {
        var hasChanged = false;
        foreach (var i in instructions)
        {
            if (i.opcode == OpCodes.Ldc_I4_1 && !hasChanged)
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_0); // Change 1 to 0
                hasChanged = true; // Ensure we only change the first occurrence, the next one is for another logic
            }
            else
            {
                yield return i;
            }
        }
    }

}
