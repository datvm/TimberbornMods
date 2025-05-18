namespace Ziporter.Services;

public class ZiporterNavGroupService(NavMeshGroupService navMeshGroupService) : ILoadableSingleton, IUnloadableSingleton
{
    public static ZiporterNavGroupService? Instance { get; private set; }

    public int GroupId { get; private set; }

    public void Load()
    {
        GroupId = navMeshGroupService.GetOrAddGroupId("Ziporter");
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}
