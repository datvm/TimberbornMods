namespace NoBuildRestriction.Patches;

public static class HangingTerrainPatches
{

    public static void PatchHangingTerrain()
    {
        var max = MSettings.SuperHangingTerrainLimit;
        
        typeof(TerrainPhysicsValidator).Field(nameof(TerrainPhysicsValidator.MaxSupportDistanceDoubled)).SetValue(null, max * 2);
        typeof(TerrainPhysicsValidator).Field(nameof(TerrainPhysicsValidator.MaxSupportDistance)).SetValue(null, max);
    }

}
