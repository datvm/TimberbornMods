namespace ConfigurableFaction.Models;

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

    public FrozenDictionary<string, TItemSpec> ItemSpecsByIds { get; private set; } = FrozenDictionary<string, TItemSpec>.Empty;

    public abstract string? GetCollectionId(TCollectionSpec spec);
    public abstract IEnumerable<TItem> GetItems(TCollectionSpec spec);
    public abstract string GetItemSpecId(TItemSpec item);
    public abstract string GetItemId(TItem item);
    public abstract TCollection CreateCollection(string id, List<TItem> items);

    public FrozenDictionary<string, TItem> ItemsByIds { get; protected set; } = FrozenDictionary<string, TItem>.Empty;
    public FrozenDictionary<string, TCollection> CollectionsByIds { get; protected set; } = FrozenDictionary<string, TCollection>.Empty;

    protected readonly ISpecService specs = null!;

    protected AggregatedCollectionBase() { }
    public AggregatedCollectionBase(ISpecService specs)
    {
        this.specs = specs;
    }

    public virtual void Aggregate()
    {
        ItemSpecsByIds = specs.GetSpecs<TItemSpec>().ToFrozenDictionary(GetItemSpecId);

        Dictionary<string, List<TItem>> itemsGroups = [];
        Dictionary<string, TItem> itemsByIds = [];

        foreach (var spec in specs.GetSpecs<TCollectionSpec>())
        {
            var colId = GetCollectionId(spec);
            if (colId is null) { continue; }

            var grp = itemsGroups.GetOrAdd(colId);
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

    public IEnumerable<TItem> CommonItems => CollectionsByIds.TryGetValue(ConfigurableFactionUtils.CommonCollectionId, out var items)
            ? items.Items
            : [];

    public HashSet<string> CommonItemsIds => [.. CommonItems.Select(GetItemId)];

}
