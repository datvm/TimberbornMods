using HarmonyLib;
using Timberborn.Bots;
using Timberborn.WalkingSystem;

namespace ConfigurableBeaverWalk
{

    [HarmonyPatch(typeof(WalkerSpeedManager), "UpdateSpeed")]
    public static class WalkerSpeedManagerPatch
    {

        public static void Prefix(WalkerSpeedManager __instance, ref float ____baseWalkingSpeed, ref float ____baseSlowedSpeed)
        {
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

    }

}
