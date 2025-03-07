namespace BuffDebuff;

public abstract class EntityBasedBuffTarget(EventBus eventBus) : IBuffTarget
{
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

public abstract class GlobalBuffTarget(IBuffableService buffables, EventBus eventBus) : IBuffTarget
{
    public long Id { get; set; }
    public IEnumerable<BuffableComponent> Targets { get; private set; } = [];

    protected bool Dirty { get; set; } = true;
    public bool TargetsChanged { get; private set; }

    protected abstract bool Filter(BuffableComponent buffable);

    public void Init()
    {
        eventBus.Register(this);
    }

    public virtual void UpdateTargets()
    {
        TargetsChanged = false;
        if (!Dirty) { return; }

        Dirty = false;
        Targets = buffables.Buffables
            .Where(b =>
            {
                try
                {
                    return b?._componentCache is not null && Filter(b);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error filtering buffable {b}: {ex}");
                    return false;
                }
            })
            .ToHashSet();
        TargetsChanged = true;
    }

    [OnEvent]
    public void OnEntityInitializedEvent(EntityInitializedEvent e)
    {
        if (Filter(e.Entity.GetBuffable()))
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

public class GlobalBeaverBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{

    protected override bool Filter(BuffableComponent buffable)
    {
        return buffable.GetComponentFast<BeaverSpec>();
    }

}

public class GlobalAdultBeaverBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{
    protected override bool Filter(BuffableComponent buffable)
    {
        return buffable.GetComponentFast<AdultSpec>();
    }
}

public class GlobalChildBeaverBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{
    protected override bool Filter(BuffableComponent buffable)
    {
        return buffable.GetComponentFast<ChildSpec>();
    }
}

public class GlobalBotBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{

    protected override bool Filter(BuffableComponent buffable)
    {
        return buffable.GetComponentFast<BotSpec>();
    }

}