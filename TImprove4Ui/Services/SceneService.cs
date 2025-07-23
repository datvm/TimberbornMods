namespace TImprove4Ui.Services;

public class SceneService(ISceneLoader sceneLoader) : ILoadableSingleton, IUnloadableSingleton
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
