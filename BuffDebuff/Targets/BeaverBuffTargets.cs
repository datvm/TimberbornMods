namespace BuffDebuff;

public abstract class BaseGlobalBeaverBuffTarget<TSpec>(EventBus eventBus, BeaverPopulation beaverPops) : EntityBasedBuffTarget(eventBus)
    where TSpec : BaseComponent
{
    protected readonly BeaverPopulation beaverPops = beaverPops;

    protected override bool Filter(EntityComponent entity) => entity.GetComponentFast<TSpec>();

    protected override HashSet<BuffableComponent> GetTargets() => [.. GetSpec(beaverPops._beaverCollection).Select(q => q.GetBuffable())];
    protected abstract IEnumerable<BeaverSpec> GetSpec(BeaverCollection c);
}

public class GlobalBeaverBuffTarget(EventBus eventBus, BeaverPopulation beaverPops) : BaseGlobalBeaverBuffTarget<BeaverSpec>(eventBus, beaverPops)
{
    protected override IEnumerable<BeaverSpec> GetSpec(BeaverCollection c) => c.Beavers;
}


public class GlobalAdultBeaverBuffTarget(EventBus eventBus, BeaverPopulation beaverPops) : BaseGlobalBeaverBuffTarget<AdultSpec>(eventBus, beaverPops)
{
    protected override IEnumerable<BeaverSpec> GetSpec(BeaverCollection c) => c.Adults;
}

public class GlobalChildBeaverBuffTarget(EventBus eventBus, BeaverPopulation beaverPops) : BaseGlobalBeaverBuffTarget<ChildSpec>(eventBus, beaverPops)
{
    protected override IEnumerable<BeaverSpec> GetSpec(BeaverCollection c) => c.Children;
}