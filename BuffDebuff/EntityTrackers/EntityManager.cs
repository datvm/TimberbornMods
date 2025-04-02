namespace BuffDebuff;

public record class TrackingEntityEvent(EntityComponent Entity, HashSet<BaseComponent> TrackingComponents);
public record class TrackingEntityInitializedEvent(EntityComponent Entity, HashSet<BaseComponent> TrackingComponents) : TrackingEntityEvent(Entity, TrackingComponents);
public record class TrackingEntityDeletedEvent(EntityComponent Entity, HashSet<BaseComponent> TrackingComponents) : TrackingEntityEvent(Entity, TrackingComponents);

public class EntityManager(EventBus eb, EntityRegistry registry, IEnumerable<ITrackingEntities> trackingEntities) : ILoadableSingleton
{

    FrozenDictionary<Type, EntityManagerType> entitiesDict = null!;
    readonly Dictionary<EntityComponent, HashSet<BaseComponent>> entities = [];

    public IReadOnlyCollection<EntityComponent> Entities => entities.Keys;

    EntityManagerType GetList(Type type)
    {
        if (!entitiesDict.TryGetValue(type, out var list))
        {
            throw new InvalidOperationException($"{type} was not registered to be tracked. Use {nameof(ITrackingEntities)} to register it.");
        }
        return list;
    }

    public ReadOnlyHashSet<T> Get<T>() where T : BaseComponent
    {
        var list = GetList(typeof(T));
        return list.Get<T>();
    }

    public int Count<T>() where T : BaseComponent
    {
        return GetList(typeof(T)).Count;
    }

    public ReadOnlyHashSet<BaseComponent> GetTrackingComponents(EntityComponent entity)
    {
        return entities.TryGetValue(entity, out var list) ? list.AsReadOnly() : default;
    }

    public void Load()
    {
        entitiesDict = trackingEntities
            .SelectMany(q => q.TrackingTypes)
            .Distinct()
            .ToFrozenDictionary(q => q, EntityManagerType.Create);

        foreach (var e in registry.Entities)
        {
            var entity = e.GetComponentFast<EntityComponent>();
            if (entity) { AddEntity(entity); }
        }

        eb.Register(this);
    }

    void AddEntity(EntityComponent entity)
    {
        if (entities.ContainsKey(entity)) { return; }

        var comps = new HashSet<BaseComponent>();

        foreach (var list in entitiesDict.Values)
        {
            var comp = (BaseComponent)list.GetComponentFast.Invoke(entity, []);
            if (comp is not null)
            {
                list.Add(comp);
                comps.Add(comp);
            }
        }

        if (comps.Any())
        {
            entities.Add(entity, comps);
            eb.Post(new TrackingEntityInitializedEvent(entity, comps));
        }
    }

    void RemoveEntity(EntityComponent entity)
    {
        if (!entities.TryGetValue(entity, out var list)) { return; }

        foreach (var comp in list)
        {
            entitiesDict[comp.GetType()].Remove(comp);
        }

        entities.Remove(entity);
        eb.Post(new TrackingEntityDeletedEvent(entity, list));
    }

    [OnEvent]
    public void OnEntityInitialized(EntityInitializedEvent e)
    {
        AddEntity(e.Entity);
    }

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent e)
    {
        RemoveEntity(e.Entity);
    }

}

abstract class EntityManagerType(Type type)
{
    public Type Type => type;
    public MethodBase GetComponentFast = typeof(BaseComponent).GetMethod(nameof(BaseComponent.GetComponentFast)).MakeGenericMethod(type);

    public abstract void Add(BaseComponent comp);
    public abstract void Remove(BaseComponent comp);
    public abstract ReadOnlyHashSet<T> Get<T>() where T : BaseComponent;
    public abstract int Count { get; }

    public static EntityManagerType Create(Type t)
    {
        var type = typeof(EntityManagerType<>).MakeGenericType(t);
        return (EntityManagerType)Activator.CreateInstance(type, [t])!;
    }
}

class EntityManagerType<T>(Type type) : EntityManagerType(type)
    where T : BaseComponent
{
    public HashSet<T> Entities { get; } = [];

    public override void Add(BaseComponent comp)
    {
        Entities.Add((T)comp);
    }

    public override ReadOnlyHashSet<T1> Get<T1>()
    {
        return ((HashSet<T1>)(object)Entities).AsReadOnly();
    }

    public override int Count => Entities.Count;

    public override void Remove(BaseComponent comp)
    {
        Entities.Remove((T)comp);
    }
}