namespace TheArchitectsToolkit.Services;

public class ToolkitGameService : IUnloadableSingleton
{
    public static ToolkitGameService? Instance { get; private set; }

    readonly BuildingUnlockingService buildingUnlocking;

    public ToolkitGameService(BuildingUnlockingService buildingUnlocking)
    {
        Instance = this;

        this.buildingUnlocking = buildingUnlocking;
    }

    public void LockAllUnlockedBuildings()
    {
        buildingUnlocking._unlockedBuildings.Clear();
    }

    public void Unload()
    {
        Instance = null;
    }

}
