global using Timberborn.CharacterMovementSystem;
global using Timberborn.TubeSystem;

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

}
