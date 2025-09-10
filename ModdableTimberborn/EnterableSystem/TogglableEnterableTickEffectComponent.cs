namespace ModdableTimberborn.EnterableSystem;

public abstract class TogglableEnterableTickEffectComponent : TickableComponent, ITogglableContainer<Enterable, Enterer>
{
    TogglableEnterableContainer data = null!;

    public Enterable Container => data.Container;
    public IEnumerable<Enterer> Members => data.Members;
    public bool Active => data.Active;

    public virtual void Awake() => data = new(GetComponentFast<Enterable>());

    public override void Tick()
    {
        if (!Active) { return; }
        TickEffect();
    }
    protected abstract void TickEffect();
    public void Toggle(bool active) => data.Toggle(active);

}

public class TogglableEnterableContainer(Enterable enterable) : BaseTogglableContainer<Enterable, Enterer>
{
    public override Enterable Container { get; } = enterable;
    public override IEnumerable<Enterer> Members => Container.EnterersInside;

    protected override void PerformActivate() { }
    protected override void PerformDeactivate() { }
}
