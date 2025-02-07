global using Timberborn.ScienceSystem;

namespace TheArchitectsToolkit;

public class ToolkitGameService : IUnloadableSingleton
{
    public static ToolkitGameService? Instance { get; private set; }

    static readonly FieldInfo UnlockedBuildingsField = typeof(BuildingUnlockingService).Field("_unlockedBuildings");
    readonly BuildingUnlockingService buildingUnlocking;

    public ToolkitGameService(BuildingUnlockingService buildingUnlocking)
    {
        Instance = this;

        this.buildingUnlocking = buildingUnlocking;
    }

    public void LockAllUnlockedBuildings()
    {
        var unlockedBuildings = UnlockedBuildingsField.GetValue(buildingUnlocking) as HashSet<string>
            ?? throw new ArgumentNullException("unlockedBuildings");
        unlockedBuildings.Clear();
    }

    public void Unload()
    {
        Instance = null;
    }
}
