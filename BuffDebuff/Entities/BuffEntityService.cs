namespace BuffDebuff;

public class BuffEntityService(ISingletonLoader loader) : ISaveableSingleton, ILoadableSingleton, IBuffEntityService
{

    static readonly SingletonKey SaveKey = new("BuffEntity");
    static readonly PropertyKey<string> UuidKey = new("Uuid");

    long globalUuid;

    readonly Dictionary<long, IBuffEntity> entites = [];

    public long NewUuid() => globalUuid++;

    public void Register<T>(T entity) where T : IBuffEntity
    {
        if (entity.Id == 0)
        {
            entity.Id = NewUuid();
        }
        else if (entites.ContainsKey(entity.Id))
        {
            throw new InvalidOperationException($"Entity with UUID {entity.Id} already exists.");
        }
        else if (entity.Id >= globalUuid)
        {
            globalUuid = entity.Id + 1;
        }

        entites.Add(entity.Id, entity);
    }

    public void Unregister<T>(T entity) where T : IBuffEntity
    {
        entites.Remove(entity.Id);
    }

    public IBuffEntity Get(long uuid)
    {
        if (entites.TryGetValue(uuid, out var entity))
        {
            return entity;
        }
        else
        {
            throw new InvalidOperationException($"Entity with UUID {uuid} does not exist.");
        }
    }

    public T Get<T>(long uuid) where T : IBuffEntity
    {
        return (T)Get(uuid);
    }

    public void Load()
    {
        LoadUuid();
    }

    void LoadUuid()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }

        var s = loader.GetSingleton(SaveKey);
        globalUuid = long.Parse(s.Get(UuidKey));
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(UuidKey, globalUuid.ToString());
    }

}
