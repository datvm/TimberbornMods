namespace ConfigurableFaction.Models;

public abstract class OrderedSpecCollectionBase<TSpec, TDef>
    where TSpec : ComponentSpec
{
    readonly ISpecService specs = null!;

    public abstract string GetId(TDef spec);
    public abstract int GetOrder(TSpec spec);
    public abstract TDef CreateItem(TSpec spec);

    public ImmutableArray<TDef> Items { get; private set; } = [];
    public FrozenDictionary<string, TDef> ItemsById { get; private set; } = FrozenDictionary<string, TDef>.Empty;

    protected OrderedSpecCollectionBase() { }

    public OrderedSpecCollectionBase(ISpecService specs)
    {
        this.specs = specs;
    }

    public void Aggregate()
    {
        Items = [.. specs.GetSpecs<TSpec>()
            .OrderBy(GetOrder)
            .Select(CreateItem)];
        ItemsById = Items.ToFrozenDictionary(GetId);
    }

}
