
namespace ModdableTimberborn.EnterableSystem;

public abstract class TogglableEnterableTickEffectComponent : TickableComponent, ITogglableContainer<Enterable, Enterer>
{
    TogglableEnterableContainer data = null!;

    public event Action<bool>? Toggled;

    public Enterable Container => data.Container;
    public IEnumerable<Enterer> Members => data.Members;
    public bool Active => data.Active;

    public virtual void Awake()
    {
        data = new(GetComponentFast<Enterable>());
        data.Toggled += e => Toggled?.Invoke(e);
    }

    public override void Tick()
    {
        if (!Active) { return; }
        TickEffect();
    }
    protected abstract void TickEffect();
    public void Toggle(bool active) => data.Toggle(active);
}

public abstract class TogglableEnterableTickEffectComponent<TData> : TogglableEnterableTickEffectComponent
{

    protected abstract TData GetData(Enterer enterer);

    protected Dictionary<Enterer, TData> enterers = [];
    public IReadOnlyCollection<KeyValuePair<Enterer, TData>> Enterers => enterers;
    public IReadOnlyCollection<TData> EnterersData => enterers.Values;

    public override void Awake()
    {
        base.Awake();

        Container.EntererAdded += Container_EntererAdded;
        Container.EntererRemoved += Container_EntererRemoved;
        foreach (var m in Members)
        {
            enterers.Add(m, GetData(m));
        }
    }

    void Container_EntererRemoved(object sender, EntererRemovedEventArgs e)
    {
        enterers.Remove(e.Enterer);
    }

    void Container_EntererAdded(object sender, EntererAddedEventArgs e)
    {
        enterers.Add(e.Enterer, GetData(e.Enterer));
    }

}

public class TogglableEnterableContainer(Enterable enterable) : BaseTogglableContainer<Enterable, Enterer>
{
    public override Enterable Container { get; } = enterable;
    public override IEnumerable<Enterer> Members => Container.EnterersInside;

    protected override void PerformActivate() { }
    protected override void PerformDeactivate() { }
}
