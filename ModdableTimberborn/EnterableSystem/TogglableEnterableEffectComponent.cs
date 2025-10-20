namespace ModdableTimberborn.EnterableSystem;

public abstract class TogglableEnterableEffectComponent : BaseEventTogglableContainerComponent<TogglableEventEnterableContainer, Enterable, Enterer>
{

    protected abstract void OnEnter(Enterer enterer);
    protected abstract void OnExit(Enterer enterer);

    protected override TogglableEventEnterableContainer CreateData(Enterable comp) => new(comp, OnEnter, OnExit);

}

/// <summary>
/// A togglable component that applies/removes bonuses to/from enterers when they enter/exit the enterable.
/// Must have <see cref="ModdableTimberbornRegistry.UseBonusTracker(bool)"/> registered.
/// </summary>
public abstract class TogglableEnterableBonusComponent : TogglableEnterableEffectComponent
{
    protected abstract BonusTrackerItem Bonuses { get; }

    protected override void OnEnter(Enterer enterer)
    {
        enterer.GetBonusTracker().AddOrUpdate(Bonuses);
    }

    protected override void OnExit(Enterer enterer)
    {
        enterer.GetBonusTracker().Remove(Bonuses.Id);
    }
}

public class TogglableEventEnterableContainer(Enterable enterable, Action<Enterer> onEnter, Action<Enterer> onExit) : BaseEventTogglableContainer<Enterable, Enterer>
{
    public override Enterable Container { get; } = enterable;
    public override IEnumerable<Enterer> Members => Container.EnterersInside;

    protected override void AddEventHandlers()
    {
        Container.EntererAdded += Container_EntererAdded;
        Container.EntererRemoved += Container_EntererRemoved;
    }

    protected override void RemoveEventHandlers()
    {
        Container.EntererAdded -= Container_EntererAdded;
        Container.EntererRemoved -= Container_EntererRemoved;
    }

    void Container_EntererRemoved(object sender, EntererRemovedEventArgs e) => OnMemberRemoved(e.Enterer);
    void Container_EntererAdded(object sender, EntererAddedEventArgs e) => OnMemberAdded(e.Enterer);

    protected override void OnMemberAdded(Enterer member) => onEnter(member);
    protected override void OnMemberRemoved(Enterer member) => onExit(member);

}