namespace ConfigurableFaction.Definitions;

public abstract class OrderedSpecCollectionBase<TSpec, TDef>
    where TSpec : ComponentSpec
{

    public abstract string GetId(TDef spec);
    public abstract int GetOrder(TSpec spec);
    public abstract TDef CreateItem(TSpec spec);

    public ImmutableArray<TDef> Items { get; }
    public FrozenDictionary<string, TDef> ItemsById { get; }

    protected OrderedSpecCollectionBase()
    {
        Items = [];
        ItemsById = FrozenDictionary<string, TDef>.Empty;
    }

    public OrderedSpecCollectionBase(ISpecService specs)
    {
        Items = [.. specs.GetSpecs<TSpec>()
            .OrderBy(GetOrder)
            .Select(CreateItem)];
        ItemsById = Items.ToFrozenDictionary(GetId);
    }

}
