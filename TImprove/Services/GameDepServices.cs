namespace TImprove.Services;

public class GameDepServices(
    EntityService entities
) : ILoadableSingleton, IUnloadableSingleton
{
    public static GameDepServices? Instance { get; private set; }

    public void DeleteObject(BlockObject obj)
    {
        entities.Delete(obj);
    }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}
