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

    public virtual bool AllowBeavers => true;
    public virtual bool AllowBots => true;
    public abstract HashSet<Type> Workplaces { get; }

    protected override bool Filter(EntityComponent entity) => Filter(entity);

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

    [OnEvent]
    public void OnWorkerChanged(WorkerChangedEventArgs ev)
    {
        if (Filter(ev.Worker))
        {
            Dirty = true;
        }
    }

}

public class GlobalBuilderBuffTarget(EventBus eventBus, BeaverPopulation beaverPops, BotPopulation botPops) : GlobalWorkplaceBuffTarget(eventBus, beaverPops, botPops)
{
    public override HashSet<Type> Workplaces => [typeof(DistrictCenter), typeof(BuilderHubSpec)];
}
