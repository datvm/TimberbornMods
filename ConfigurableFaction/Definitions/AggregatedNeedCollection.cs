namespace ConfigurableFaction.Definitions;

public class AggregatedNeedCollection : AggregatedCollectionBase<NeedCollectionSpec, NeedSpec, NeedSpec>
{
    public static readonly AggregatedNeedCollection Empty = new();

    AggregatedNeedCollection() : base() { }
    public AggregatedNeedCollection(ISpecService specs) : base(specs) { }

    public override CollectionDefBase<NeedSpec> CreateCollection(string id, List<NeedSpec> items) => new(id, [.. items]);

    public override string GetCollectionId(NeedCollectionSpec spec) => spec.CollectionId;
    public override string GetItemId(NeedSpec item) => item.Id;
    public override string GetItemSpecId(NeedSpec item) => item.Id;
    public override IEnumerable<NeedSpec> GetItems(NeedCollectionSpec spec)
        => spec.Needs.Select(id => ItemSpecsByIds[id]);

}
