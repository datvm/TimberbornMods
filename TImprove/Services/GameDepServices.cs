namespace TImprove.Services;

public class GameDepServices
{
    public static GameDepServices? Instance { get; private set; }

    readonly EntityService entities;

    public GameDepServices(EntityService entities)
    {
        Instance = this;

        this.entities = entities;
    }

    public void DeleteObject(BlockObject obj)
    {
        entities.Delete(obj);
    }

}
