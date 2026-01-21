namespace TImprove4Ui.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class StaticSingletonsService(
    EntityBadgeService entityBadgeService,
    ISceneLoader sceneLoader
) : ILoadableSingleton, IUnloadableSingleton
{

    public static EntityBadgeService? EntityBadgeService { get; private set; }
    public static ISceneLoader? SceneLoader { get; private set; }

    public void Load()
    {
        EntityBadgeService = entityBadgeService;
        SceneLoader = sceneLoader;
    }

    public void Unload()
    {
        EntityBadgeService = null;
        SceneLoader = null;
    }
}
