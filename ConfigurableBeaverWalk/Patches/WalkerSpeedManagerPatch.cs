namespace ConfigurableBeaverWalk.Patches;

[HarmonyPatch]
public static class WalkerSpeedManagerPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(WalkerSpeedManager), nameof(WalkerSpeedManager.Tick))]
    public static void TickPrefix(WalkerSpeedManager __instance, ref float ____baseSpeed, IMovementSpeedAffector ____movementSpeedAffector)
    {
        if (!MSettings.ChangeWalkingSpeed) { return; }

        var walkingSpeed = MSettings.BaseWalkingSpeed;
        var slowedSpeed = MSettings.BaseSlowedSpeed;

        if (MSettings.DifferentForBots)
        {
            var isBot = __instance.HasComponent<BotSpec>();

            if (isBot)
            {
                walkingSpeed = MSettings.BaseBotWalkingSpeed;
                slowedSpeed = MSettings.BaseBotSlowedSpeed;
            }
        }

        ____baseSpeed = ____movementSpeedAffector?.IsMovementSlowed == true ?
            slowedSpeed :
            walkingSpeed;
    }
}
