namespace ConfigurableFaction.Definitions;

public abstract class AggregatedCollectionBase<TCollectionSpec, TItemSpec, TItem> : AggregatedCollectionBase<TCollectionSpec, TItemSpec, CollectionDefBase<TItem>, TItem>
    where TCollectionSpec : ComponentSpec
    where TItemSpec : ComponentSpec
{
    public AggregatedCollectionBase(ISpecService specs) : base(specs) { }
    protected AggregatedCollectionBase() { }
}

public abstract class AggregatedCollectionBase<TCollectionSpec, TItemSpec, TCollection, TItem>
    where TCollectionSpec : ComponentSpec
    where TItemSpec : ComponentSpec
    where TCollection : CollectionDefBase<TItem>
{
    public FrozenDictionary<string, TItemSpec> ItemSpecsByIds { get; }

    public abstract string GetCollectionId(TCollectionSpec spec);
    public abstract IEnumerable<TItem> GetItems(TCollectionSpec spec);
    public abstract string GetItemSpecId(TItemSpec item);
    public abstract string GetItemId(TItem item);
    public abstract TCollection CreateCollection(string id, List<TItem> items);

    public FrozenDictionary<string, TItem> ItemsByIds { get; }
    public FrozenDictionary<string, TCollection> CollectionsByIds { get; }

    protected AggregatedCollectionBase()
    {
        ItemSpecsByIds = FrozenDictionary<string, TItemSpec>.Empty;
        ItemsByIds = FrozenDictionary<string, TItem>.Empty;
        CollectionsByIds = FrozenDictionary<string, TCollection>.Empty;
    }

    public AggregatedCollectionBase(ISpecService specs)
    {
        ItemSpecsByIds = specs.GetSpecs<TItemSpec>().ToFrozenDictionary(GetItemSpecId);

        Dictionary<string, List<TItem>> itemsGroups = [];
        Dictionary<string, TItem> itemsByIds = [];

        foreach (var spec in specs.GetSpecs<TCollectionSpec>())
        {
            var id = GetCollectionId(spec);
            var grp = itemsGroups.GetOrAdd(id);

            foreach (var item in GetItems(spec))
            {
                grp.Add(item);

                var itemId = GetItemId(item);
                if (!itemsByIds.ContainsKey(itemId))
                {
                    itemsByIds[itemId] = item;
                }
            }
        }

        ItemsByIds = itemsByIds.ToFrozenDictionary();
        CollectionsByIds = itemsGroups.ToFrozenDictionary(
            kv => kv.Key,
            kv => CreateCollection(kv.Key, kv.Value));
    }

    public IEnumerable<TItem> GetByCollectionIds(IEnumerable<string> collectionIds)
        => collectionIds.SelectMany(id => CollectionsByIds[id].Items);

}
