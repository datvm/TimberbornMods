using HarmonyLib;
using Timberborn.WalkingSystem;

namespace ConfigurableBeaverWalk
{

    [HarmonyPatch(typeof(WalkerSpeedManager), "UpdateSpeed")]
    public static class WalkerSpeedManagerPatch
    {

        public static void Prefix(ref float ____baseWalkingSpeed, ref float ____baseSlowedSpeed)
        {
            if (____baseWalkingSpeed < ModSettings.BaseWalkingSpeed)
            {
                ____baseWalkingSpeed = ModSettings.BaseWalkingSpeed;
            }

            if (____baseSlowedSpeed < ModSettings.BaseSlowedSpeed)
            {
                ____baseSlowedSpeed = ModSettings.BaseSlowedSpeed;
            }
        }

    }

}
