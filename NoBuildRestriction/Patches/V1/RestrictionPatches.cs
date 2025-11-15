namespace NoBuildRestriction.Patches;

[HarmonyPatch]
public static class RestrictionPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BlockableFloodableObject), nameof(BlockableFloodableObject.OnFlooded))]
    public static bool PreventFloodBlocking()
        => !MSettings.AllowFlooded;

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainPhysicsPostLoader), nameof(TerrainPhysicsPostLoader.RemoveBlockObjects))]
    public static bool DontRemoveBlockObjects() 
        => !MSettings.SuperHangingTerrain && (!MSettings.SuperStructure || !MSettings.MagicStructure);

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainPhysicsPostLoader), nameof(TerrainPhysicsPostLoader.RemoveTerrain))]
    public static bool DontRemoveTerrain() 
        => DontRemoveBlockObjects();

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainLevelValidationConfigurator), nameof(TerrainLevelValidationConfigurator.ProvideTemplateModule))]
    public static bool RemoveContinuousTerrainConstraint(ref TemplateModule __result)
    {
        if (!MSettings.NoBottomOfMap) { return true; }

        __result = new TemplateModule.Builder().Build();
        return false;
    }

}
