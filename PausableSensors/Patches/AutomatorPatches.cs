global using System.Reflection.Emit;

namespace PausableSensors.Patches;

[HarmonyPatch(typeof(Automator))]
public class AutomatorPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(Automator.ValidateAwake))]
    public static IEnumerable<CodeInstruction> AllowBothTerminalAndTransmitter(IEnumerable<CodeInstruction> instructions)
    {
        var firstThrow = true;

        foreach (var ins in instructions)
        {
            if (firstThrow && ins.opcode == OpCodes.Throw)
            {
                firstThrow = false;
                yield return new(OpCodes.Pop);
            }
            else
            {
                yield return ins;
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(Automator.State), MethodType.Getter)]
    public static bool GetPausableState(Automator __instance, ref AutomatorState __result)
    {
        if (AutomatorPausable.PausedAutomators.TryGetValue(__instance, out _))
        {
            __result = AutomatorState.Off;
            return false;
        }

        return true;
    }

}
