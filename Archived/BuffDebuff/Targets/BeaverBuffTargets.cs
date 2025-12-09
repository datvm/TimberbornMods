namespace BuffDebuff;

public abstract class BaseBeaverBuffTarget<TSpec>(EventBus eventBus, BeaverPopulation beaverPops) : EntityBasedBuffTarget(eventBus)
    where TSpec : BaseComponent
{
    protected readonly BeaverPopulation beaverPops = beaverPops;

    protected override bool Filter(EntityComponent entity) => entity.GetComponentFast<TSpec>();

    protected override HashSet<BuffableComponent> GetTargets() => [.. GetSpec(beaverPops._beaverCollection).Select(q => q.GetBuffable())];
    protected abstract IEnumerable<BeaverSpec> GetSpec(BeaverCollection c);
}

public class BeaverBuffTarget(EventBus eventBus, BeaverPopulation beaverPops) : BaseBeaverBuffTarget<BeaverSpec>(eventBus, beaverPops)
{
    protected override IEnumerable<BeaverSpec> GetSpec(BeaverCollection c) => c.Beavers;
}

public class AdultBeaverBuffTarget(EventBus eventBus, BeaverPopulation beaverPops) : BaseBeaverBuffTarget<AdultSpec>(eventBus, beaverPops)
{
    protected override IEnumerable<BeaverSpec> GetSpec(BeaverCollection c) => c.Adults;
}

public class ChildBeaverBuffTarget(EventBus eventBus, BeaverPopulation beaverPops) : BaseBeaverBuffTarget<ChildSpec>(eventBus, beaverPops)
{
    protected override IEnumerable<BeaverSpec> GetSpec(BeaverCollection c) => c.Children;
}

public enum BeaverTarget
{
    All = 0,
    Adult = 1,
    Child = 2,
}