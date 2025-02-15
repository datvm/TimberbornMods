global using Timberborn.CharacterMovementSystem;
global using Timberborn.TubeSystem;
using Timberborn.WalkingSystem;

namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch]
public static class SpeedPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MovementSpeedBoostingBuildingSpec), nameof(MovementSpeedBoostingBuildingSpec.BoostPercentage), MethodType.Getter)]
    public static bool ChangeSpeedBoostUi(MovementSpeedBoostingBuildingSpec __instance, ref int __result)
    {
        if (__instance.GetComponentFast<ZiplineTower>())
        {
            __result = MSettings.ZiplineSpeed;
            return false;
        }
        else if (__instance.GetComponentFast<TubeStationSpec>())
        {
            __result = MSettings.TubewaySpeed;
            return false;
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WalkerSpeedManager), nameof(WalkerSpeedManager.GetWalkerSpeedAtCurrentPosition))]
    public static bool RemoveWaterPenalty(WalkerSpeedManager __instance, ref float __result)
    {
        if (!MSettings.NoWaterPenalty) { return true; }

        __result = __instance.GetWalkerBaseSpeed();
        return false;
    }

}
