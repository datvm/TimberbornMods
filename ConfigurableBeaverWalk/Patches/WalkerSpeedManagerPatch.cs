namespace ConfigurableBeaverWalk.Patches;

[HarmonyPatch]
public static class WalkerSpeedManagerPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(WalkerSpeedManager), nameof(WalkerSpeedManager.Tick))]
    public static void TickPrefix(WalkerSpeedManager __instance, ref float ____baseSpeed, IMovementSpeedAffector ____movementSpeedAffector)
    {
        if (!ModSettings.ChangeWalkingSpeed) { return; }

        var walkingSpeed = ModSettings.BaseWalkingSpeed;
        var slowedSpeed = ModSettings.BaseSlowedSpeed;

        if (ModSettings.DifferentForBots)
        {
            var isBot = __instance.GetComponentFast<BotSpec>() is not null;

            if (isBot)
            {
                walkingSpeed = ModSettings.BaseBotWalkingSpeed;
                slowedSpeed = ModSettings.BaseBotSlowedSpeed;
            }
        }

        ____baseSpeed = ____movementSpeedAffector?.IsMovementSlowed == true ?
            slowedSpeed :
            walkingSpeed;
    }
}
