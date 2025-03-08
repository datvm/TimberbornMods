namespace BuffDebuff;

public interface ITrackingWorkplace
{
    IEnumerable<Type> TrackingTypes { get; }
}

public class WorkplaceManager(EventBus eb, EntityRegistry registry, IEnumerable<ITrackingWorkplace> trackingWorkplaces) : ILoadableSingleton
{

    FrozenDictionary<Type, WorkplaceManagerType> workplacesDict = null!;
    readonly HashSet<Workplace> workplaces = [];

    public ReadOnlyHashSet<Workplace> Workplaces => workplaces.AsReadOnly();
    
    public ReadOnlyHashSet<T> Get<T>() where T : BaseComponent
    {
        if (!workplacesDict.TryGetValue(typeof(T), out var list))
        {
            throw new InvalidOperationException($"{typeof(T)} was not registered to be tracked. Use {nameof(ITrackingWorkplace)} to register it.");
        }

        return list.Get<T>();
    }

    public void Load()
    {
        workplacesDict = trackingWorkplaces
            .SelectMany(q => q.TrackingTypes)
            .Distinct()
            .ToFrozenDictionary(q => q, WorkplaceManagerType.Create);

        foreach (var e in registry.Entities)
        {
            var wp = e.GetComponentFast<Workplace>();
            if (wp) { AddWorkplace(wp); }
        }

        eb.Register(this);
    }

    void AddWorkplace(Workplace workplace)
    {
        workplaces.Add(workplace);
        foreach (var list in workplacesDict.Values)
        {
            var comp = (BaseComponent)list.GetComponentFast.Invoke(workplace, []);
            if (comp is not null)
            {
                list.Add(comp);
            }
        }
    }

    void RemoveWorkplace(Workplace workplace)
    {
        workplaces.Remove(workplace);
        foreach (var list in workplacesDict.Values)
        {
            var comp = (BaseComponent)list.GetComponentFast.Invoke(workplace, []);
            if (comp is not null)
            {
                list.Remove(comp);
            }
        }
    }

#warning Add event for when a workplace is added or removed, and maybe also when workers are changed

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent ev)
    {
        var wp = ev.BlockObject.GetComponentFast<Workplace>();
        if (wp is not null) { AddWorkplace(wp); }
    }

    [OnEvent]
    public void OnExitedFinishedState(ExitedFinishedStateEvent ev)
    {
        var wp = ev.BlockObject.GetComponentFast<Workplace>();
        if (wp is not null) { RemoveWorkplace(wp); }
    }

}

abstract class WorkplaceManagerType(Type type)
{
    public Type Type => type;
    public MethodBase GetComponentFast = typeof(BaseComponent).GetMethod(nameof(BaseComponent.GetComponentFast)).MakeGenericMethod(type);

    public abstract void Add(BaseComponent comp);
    public abstract void Remove(BaseComponent comp);
    public abstract ReadOnlyHashSet<T> Get<T>() where T : BaseComponent;

    public static WorkplaceManagerType Create(Type t)
    {
        var type = typeof(WorkplaceManagerType<>).MakeGenericType(t);
        return (WorkplaceManagerType)Activator.CreateInstance(type, [t])!;
    }
}

class WorkplaceManagerType<T>(Type type) : WorkplaceManagerType(type)
    where T : BaseComponent
{
    public HashSet<T> Workplaces { get; } = [];

    public override void Add(BaseComponent comp)
    {
        Workplaces.Add((T)comp);
    }

    public override ReadOnlyHashSet<T1> Get<T1>()
    {
        return ((HashSet<T1>)(object)Workplaces).AsReadOnly();
    }

    public override void Remove(BaseComponent comp)
    {
        Workplaces.Remove((T)comp);
    }
}