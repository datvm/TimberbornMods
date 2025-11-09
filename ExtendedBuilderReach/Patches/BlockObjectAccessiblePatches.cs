namespace ExtendedBuilderReach.Patches;

[HarmonyPatch(typeof(BlockObjectAccessible))]
public static class BlockObjectAccessiblePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObjectAccessible.InitializeEntity))]
    public static bool SkipInitializeEntity(BlockObjectAccessible __instance) => IgnoreIfHasExtendedComponent(__instance);

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObjectAccessible.OnNavMeshUpdated))]
    public static bool SkipOnNavMeshUpdated(BlockObjectAccessible __instance) => IgnoreIfHasExtendedComponent(__instance);

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObjectAccessible.SetNumberOfAccessLevelsAboveGround))]
    public static bool SkipSetNumberOfAccessLevelsAboveGround(BlockObjectAccessible __instance) 
        => IgnoreIfHasExtendedComponent(__instance);

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObjectAccessible.UpdateAccesses))]
    public static bool SkipUpdateAccesses(BlockObjectAccessible __instance) 
        => IgnoreIfHasExtendedComponent(__instance);

    public static bool IgnoreIfHasExtendedComponent(BlockObjectAccessible __instance) 
        => !__instance.HasComponent<ExtendedDemolishableAccessible>();

}
