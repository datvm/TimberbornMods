using System.Reflection.Emit;

namespace WeatherScientificProjects.Processors;

[HarmonyPatch]
public static class WeatherWarningPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(HazardousWeatherApproachingTimer), nameof(HazardousWeatherApproachingTimer.OnCycleDayStarted))]
    public static bool SkipOnCycleDayStarted() => false;

    [HarmonyTranspiler, HarmonyPatch(typeof(HazardousWeatherApproachingTimer), nameof(HazardousWeatherApproachingTimer.GetProgress))]
    public static IEnumerable<CodeInstruction> PatchGetProgress(IEnumerable<CodeInstruction> instructions)
        => ReplaceApproachingNotificationDays(instructions);

    static IEnumerable<CodeInstruction> ReplaceApproachingNotificationDays(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldsfld
                && instruction.operand.ToString().Contains("ApproachingNotificationDays"))
            {
                yield return new CodeInstruction(
                    OpCodes.Ldsfld,
                    typeof(WeatherUpgradeProcessor).Field(nameof(WeatherUpgradeProcessor.WarningDays)));
            }
            else
            {
                yield return instruction;
            }
        }
    }

}
