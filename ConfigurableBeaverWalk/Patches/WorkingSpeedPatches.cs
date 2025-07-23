namespace ConfigurableBeaverWalk.Patches;

[HarmonyPatch]
public static class WorkingSpeedPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Worker), nameof(Worker.WorkingSpeedMultiplier), MethodType.Setter)]
    public static void AddWorkSpeed(Worker __instance, ref float value)
    {
        var speed = MSettings.WorkSpeedMultiplier;

        if (MSettings.DifferentForBots && __instance.WorkerType == "Bot")
        {
            speed = MSettings.BotWorkSpeedMultiplier;
        }

        if (speed != 1)
        {
            value *= speed;
        }
    }

}
