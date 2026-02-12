namespace TImprove4Ui.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class StaticSingletonsService(
    ISceneLoader sceneLoader
) : ILoadableSingleton, IUnloadableSingleton
{

    public static ISceneLoader? SceneLoader { get; private set; }

    public void Load()
    {
        SceneLoader = sceneLoader;
    }

    public void Unload()
    {
        SceneLoader = null;
    }
}
