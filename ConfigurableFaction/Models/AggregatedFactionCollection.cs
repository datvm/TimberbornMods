namespace ConfigurableFaction.Models;

public class AggregatedFactionCollection : OrderedSpecCollectionBase<FactionSpec, FactionDef>
{
    public static readonly AggregatedFactionCollection Empty = new();
    readonly DataAggregatorService aggregator = null!;

    AggregatedFactionCollection() { }
    public AggregatedFactionCollection(ISpecService specs, DataAggregatorService aggregator) : base(specs)
    {
        this.aggregator = aggregator;
    }

    public override string GetId(FactionDef spec) => spec.Id;
    public override int GetOrder(FactionSpec spec) => spec.Order;
    public override FactionDef CreateItem(FactionSpec spec)
    {
        var templateColIds = spec.TemplateCollectionIds;

        return new(
            spec,
            [.. aggregator.Templates.GetBuildings(templateColIds)],
            [.. aggregator.Templates.GetPlants(templateColIds)],
            [.. aggregator.Goods.GetByCollectionIds(spec.GoodCollectionIds)],
            [.. aggregator.Needs.GetByCollectionIds(spec.NeedCollectionIds)]
        );
    }
}
