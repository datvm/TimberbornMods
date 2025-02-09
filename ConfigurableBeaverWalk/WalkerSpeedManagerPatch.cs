using Timberborn.Bots;
#if TIMBER7
using Timberborn.CharacterMovementSystem;
#endif
using Timberborn.WalkingSystem;

namespace ConfigurableBeaverWalk
{

    [HarmonyPatch]
    public static class WalkerSpeedManagerPatch
    {

#if TIMBER6
        [HarmonyPrefix, HarmonyPatch(typeof(WalkerSpeedManager), nameof(WalkerSpeedManager.UpdateSpeed))]
        public static void Prefix(WalkerSpeedManager __instance, ref float ____baseWalkingSpeed, ref float ____baseSlowedSpeed)
        {
            if (!ModSettings.ChangeWalkingSpeed) { return; }

            var walkingSpeed = ModSettings.BaseWalkingSpeed;
            var slowedSpeed = ModSettings.BaseSlowedSpeed;

            if (ModSettings.DifferentForBots)
            {
                var isBot = __instance.GameObjectFast.GetComponent<Bot>() is not null;

                if (isBot)
                {
                    walkingSpeed = ModSettings.BaseBotWalkingSpeed;
                    slowedSpeed = ModSettings.BaseBotSlowedSpeed;
                }
            }

            if (____baseWalkingSpeed < walkingSpeed)
            {
                ____baseWalkingSpeed = walkingSpeed;
            }

            if (____baseSlowedSpeed < slowedSpeed)
            {
                ____baseSlowedSpeed = slowedSpeed;
            }
        }
#endif

#if TIMBER7
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
#endif

    }

}
