global using Timberborn.WorkSystem;

namespace BuffDebuff.Targets;

public abstract class GlobalWorkplaceBuffTarget(EventBus eventBus) : EntityBasedBuffTarget(eventBus)
{

    public virtual bool AllowBeavers => true;
    public virtual bool AllowBots => true;

    protected override bool Filter(EntityComponent entity) => Filter((BaseComponent)entity);

    protected bool Filter(BaseComponent entity)
    {
        return
            (AllowBeavers && entity.GetComponentFast<BeaverSpec>())
            || (AllowBots && entity.GetComponentFast<BotSpec>());
    }

    protected override HashSet<BuffableComponent> GetTargets()
    {
        throw new NotImplementedException();
    }

    [OnEvent]
    public void OnWorkerChanged(WorkerChangedEventArgs ev)
    {
        if (Filter(ev.Worker))
        {
            Dirty = true;
        }
    }

}
