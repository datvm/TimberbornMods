namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(HazardousWeatherNotificationPanel))]
public static class HazardousWeatherNotificationPanelPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(HazardousWeatherNotificationPanel.OnCycleEndedEvent))]
    public static bool SkipOriginalNotification() => false;

    [HarmonyPrefix, HarmonyPatch(nameof(HazardousWeatherNotificationPanel.ShowHazardousSeasonNotification))]
    public static bool RedirectShowHazardousSeasonNotification()
    {
        ModdableHazardousWeatherNotificationPanel.Instance.NewShowHazardousSeasonNotification();
        return false;
    }

    [HarmonyTranspiler, HarmonyPatch(nameof(HazardousWeatherNotificationPanel.UpdateSingleton))]
    public static IEnumerable<CodeInstruction> RemoveUpdateSingletonLocCall(IEnumerable<CodeInstruction> instructions)
        => RemoveLocCall(instructions);

    [HarmonyTranspiler, HarmonyPatch(nameof(HazardousWeatherNotificationPanel.OnHazardousWeatherStarted))]
    public static IEnumerable<CodeInstruction> RemoveOnHazardousWeatherStartedLocCall(IEnumerable<CodeInstruction> instructions)
        => RemoveLocCall(instructions);

    // Replaces the localization call so the input key to _loc.T is passed directly to ShowHazardousSeasonNotification
    static IEnumerable<CodeInstruction> RemoveLocCall(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        for (int i = 0; i < code.Count; i++)
        {
            var ins = code[i];
            if ((ins.opcode == OpCodes.Ldfld && ins.operand.ToString().Contains("ILoc"))
                || (ins.opcode == OpCodes.Callvirt && ins.operand is MethodInfo method && method.Name == nameof(ILoc.T)))
            {
                code.RemoveAt(i);
                i--;

                if (ins.opcode == OpCodes.Ldfld)
                {
                    // Also remove the `this.` part of the call.
                    code.RemoveAt(i);
                }
            }
        }

        return code;
    }
}
