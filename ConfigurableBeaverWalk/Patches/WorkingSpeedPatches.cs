namespace ConfigurableBeaverWalk.Patches;

[HarmonyPatch]
public static class WorkingSpeedPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Worker), nameof(Worker.WorkingSpeedMultiplier), MethodType.Setter)]
    public static void AddWorkSpeed(Worker __instance, ref float value)
    {
        var speed = ModSettings.WorkSpeedMultiplier;

        if (ModSettings.DifferentForBots && __instance.WorkerType == "Bot")
        {
            speed = ModSettings.BotWorkSpeedMultiplier;
        }

        if (speed != 1)
        {
            value *= speed;
        }
    }

}
