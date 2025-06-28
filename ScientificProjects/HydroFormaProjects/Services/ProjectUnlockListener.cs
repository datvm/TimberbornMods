namespace HydroFormaProjects.Services;

public class ProjectUnlockListener(
    EventBus eb,
    TerrainBlockUpgradeService terrainService
) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnProjectPurchased(OnScientificProjectUnlockedEvent ev)
    {
        if (HydroFormaModUtils.TerrainBlockUpgrades.Contains(ev.Project.Id))
        {
            terrainService.ReloadValues();
        }
    }

}
