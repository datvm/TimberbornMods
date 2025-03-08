namespace BuffDebuff;

public abstract class EntityBasedBuffTarget(EventBus eventBus) : IBuffTarget
{
    protected readonly EventBus eventBus = eventBus;

    public long Id { get; set; }
    public IEnumerable<BuffableComponent> Targets { get; private set; } = [];

    protected bool Dirty { get; set; } = true;
    public bool TargetsChanged { get; private set; }

    protected abstract bool Filter(EntityComponent entity);
    protected abstract HashSet<BuffableComponent> GetTargets();

    public void Init()
    {
        eventBus.Register(this);
    }

    public virtual void UpdateTargets()
    {
        TargetsChanged = false;
        if (!Dirty) { return; }

        Dirty = false;
        Targets = GetTargets();
        TargetsChanged = true;
    }

    [OnEvent]
    public void OnEntityInitializedEvent(EntityInitializedEvent e)
    {
        if (Filter(e.Entity))
        {
            Dirty = true;
        }
    }

    [OnEvent]
    public void OnEntityDeletedEvent(EntityDeletedEvent e)
    {
        if (Targets.Contains(e.Entity.GetBuffable()))
        {
            Dirty = true;
        }
    }

    public void CleanUp()
    {
        eventBus.Unregister(this);
    }
}

public abstract class GlobalBuffTarget(IBuffableService buffables, EventBus eventBus) : EntityBasedBuffTarget(eventBus)
{
    protected readonly IBuffableService buffables = buffables;

    protected abstract bool Filter(BuffableComponent buffable);
    protected override bool Filter(EntityComponent entity) => Filter(entity.GetBuffable());

    protected override HashSet<BuffableComponent> GetTargets() => [.. buffables.Buffables.Where(Filter)];

}

