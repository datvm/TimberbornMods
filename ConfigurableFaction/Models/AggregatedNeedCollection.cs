namespace ConfigurableFaction.Models;

public class AggregatedNeedCollection : AggregatedCollectionBase<NeedCollectionSpec, NeedSpec, NeedDef>
{
    public static readonly AggregatedNeedCollection Empty = new();

    AggregatedNeedCollection() : base() { }
    public AggregatedNeedCollection(ISpecService specs) : base(specs) { }

    public override CollectionDefBase<NeedDef> CreateCollection(string id, List<NeedDef> items) => new(id, [.. items]);

    public override string GetCollectionId(NeedCollectionSpec spec) => spec.CollectionId;
    public override string GetItemId(NeedDef item) => item.Id;
    public override string GetItemSpecId(NeedSpec item) => item.Id;
    public override IEnumerable<NeedDef> GetItems(NeedCollectionSpec spec)
        => spec.Needs.Select(id => new NeedDef(ItemSpecsByIds[id]));

}
