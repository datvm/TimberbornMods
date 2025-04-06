global using Timberborn.TerrainPhysics;

namespace NoBuildRestriction.Patches;

public static class HangingTerrainPatches
{

    public static void PatchHangingTerrain()
    {
        const int Max = 50;

        typeof(TerrainPhysicsValidator).Field(nameof(TerrainPhysicsValidator.MaxSupportDistanceDoubled)).SetValue(null, Max * 2);
        typeof(TerrainPhysicsValidator).Field(nameof(TerrainPhysicsValidator.MaxSupportDistance)).SetValue(null, Max);

        Debug.Log("MaxSupportDistance: " + TerrainPhysicsValidator.MaxSupportDistance);
        Debug.Log(TerrainPhysicsValidator.MaxSupportDistanceDoubled);
    }

}
