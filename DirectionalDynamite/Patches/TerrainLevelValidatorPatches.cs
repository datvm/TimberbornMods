namespace DirectionalDynamite.Patches;

[HarmonyPatch]
public static class TerrainLevelValidatorPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainLevelValidator), nameof(TerrainLevelValidator.IsBottomConstraintDissatisfied))]
    public static bool SkipDynamiteBottomCheck(BlockObject blockObject, ref bool __result)
    {
        if (!blockObject.HasComponent<Dynamite>())
        {
            return true;
        }

        __result = false;
        return false;
    }

}
