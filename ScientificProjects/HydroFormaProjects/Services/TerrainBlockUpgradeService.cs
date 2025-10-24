namespace HydroFormaProjects.Services;

public class TerrainBlockUpgradeService(
    ScientificProjectUnlockRegistry unlocks,
    ScientificProjectRegistry registry
)
    : IPrefabGroupServiceTailRunner, // This is just to make sure it's load early enough
    IUnloadableSingleton
{

    public static readonly int GameDefaultValue = TerrainPhysicsValidator.MaxSupportDistance;
    
    // Don't make the following fields into properties
    public static int MaxHangingTerrain = GameDefaultValue; 
    public static int MaxHangingTerrainDoubled = GameDefaultValue * 2;

    public void ReloadValues()
    {
        var value = GameDefaultValue;

        foreach (var id in HydroFormaModUtils.TerrainBlockUpgrades)
        {
            if (unlocks.Contains(id))
            {
                var project = registry.GetProject(id);
                value += (int)project.Parameters[0];
            }
        }

        MaxHangingTerrain = value;
        MaxHangingTerrainDoubled = value * 2;
    }

    public void Run(PrefabGroupService prefabGroupService)
    {
        ReloadValues();
    }

    public void Unload()
    {
        MaxHangingTerrain = GameDefaultValue;
        MaxHangingTerrainDoubled = GameDefaultValue * 2;
    }
}
