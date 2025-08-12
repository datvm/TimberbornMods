namespace TImprove4Ui.Services;

public class StaticSingletonsService(
    EntityBadgeService entityBadgeService
) : ILoadableSingleton, IUnloadableSingleton
{

    public static EntityBadgeService? EntityBadgeService { get; private set; }

    public void Load()
    {
        EntityBadgeService = entityBadgeService;
    }

    public void Unload()
    {
        EntityBadgeService = null;
    }
}
