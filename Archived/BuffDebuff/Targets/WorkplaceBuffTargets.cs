namespace BuffDebuff;

public abstract class GlobalWorkplaceBuffTarget : EntityBasedBuffTarget
{
    readonly BeaverPopulation? beaverPops;
    readonly BotPopulation? botPops;

    public GlobalWorkplaceBuffTarget(EventBus eventBus, BeaverPopulation? beaverPops = default, BotPopulation? botPops = default) : base(eventBus)
    {
        this.beaverPops = beaverPops;
        this.botPops = botPops;

        if (AllowBeavers && beaverPops is null)
        {
            throw new ArgumentNullException(nameof(beaverPops));
        }

        if (AllowBots && botPops is null)
        {
            throw new ArgumentNullException(nameof(botPops));
        }
    }

    public virtual bool AllowBeavers { get; protected set; } = true;
    public virtual bool AllowBots { get; protected set; } = true;
    public abstract ImmutableHashSet<Type> Workplaces { get; }

    protected override bool Filter(EntityComponent entity) => Filter(entity);
    protected override bool DirtyFilter(EntityComponent entity) => DirtyFilter(entity);

    protected bool DirtyFilter(BaseComponent entity)
    {
        return (AllowBeavers && entity.GetComponentFast<BeaverSpec>())
            || (AllowBots && entity.GetComponentFast<BotSpec>());
    }

    protected bool Filter(BaseComponent entity)
    {
        var isCharacter = (AllowBeavers && entity.GetComponentFast<BeaverSpec>())
            || (AllowBots && entity.GetComponentFast<BotSpec>());
        if (!isCharacter) { return false; }

        var worker = entity.GetComponentFast<Worker>();
        var workplace = worker?.Workplace;
        if (workplace is null) { return false; }

        foreach (var comp in workplace.AllComponents)
        {
            if (Workplaces.Contains(comp.GetType()))
            {
                return true;
            }
        }

        return false;
    }

    protected override HashSet<BuffableComponent> GetTargets()
    {
        return [.. ScanTargets()];
    }

    protected virtual IEnumerable<BuffableComponent> ScanTargets()
    {
        if (AllowBeavers)
        {
            foreach (var beaver in beaverPops!._beaverCollection.Adults)
            {
                if (Filter(beaver))
                {
                    yield return beaver.GetBuffable();
                }
            }
        }

        if (AllowBots)
        {
            foreach (var bot in botPops!._bots)
            {
                if (Filter(bot))
                {
                    yield return bot.GetBuffable();
                }
            }
        }
    }

    public override void Init()
    {
        base.Init();


    }

    [OnEvent]
    public void OnWorkerChanged(WorkplaceWorkerChangedEvent ev)
    {
        if (DirtyFilter(ev.Worker))
        {
            Dirty = true;
        }
    }

}



public class GlobalBuilderBuffTarget(EventBus eventBus, BeaverPopulation beaverPops, BotPopulation botPops) : GlobalWorkplaceBuffTarget(eventBus, beaverPops, botPops)
{
    public override ImmutableHashSet<Type> Workplaces => TrackingEntityHelper.BuilderBuildingTypes;
}
