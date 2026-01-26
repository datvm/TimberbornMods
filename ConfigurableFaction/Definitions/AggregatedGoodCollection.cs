namespace ConfigurableFaction.Definitions;

public class AggregatedGoodCollection : AggregatedCollectionBase<GoodCollectionSpec, GoodSpec, GoodDef>
{
    public static readonly AggregatedGoodCollection Empty = new();

    AggregatedGoodCollection() : base() { }
    public AggregatedGoodCollection(ISpecService specs) : base(specs) { }

    public override CollectionDefBase<GoodDef> CreateCollection(string id, List<GoodDef> items) => new(id, [..items]);

    public override string GetCollectionId(GoodCollectionSpec spec) => spec.CollectionId;
    public override string GetItemId(GoodDef item) => item.Id;
    public override string GetItemSpecId(GoodSpec item) => item.Id;
    public override IEnumerable<GoodDef> GetItems(GoodCollectionSpec spec)
        => spec.Goods.Select(id => new GoodDef(ItemSpecsByIds[id]));
    
}
