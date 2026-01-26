namespace ConfigurableFaction.Definitions;

public class AggregatedFactionCollection : OrderedSpecCollectionBase<FactionSpec, FactionDef>
{
    public static readonly AggregatedFactionCollection Empty = new();
    readonly DataAggregatorService aggregator = null!;

    AggregatedFactionCollection() { }
    public AggregatedFactionCollection(ISpecService specs, DataAggregatorService dataAggregatorService) : base(specs)
    {
        this.aggregator = dataAggregatorService;
    }

    public override string GetId(FactionDef spec) => spec.Id;
    public override int GetOrder(FactionSpec spec) => spec.Order;
    public override FactionDef CreateItem(FactionSpec spec)
    {
        var ids = spec.TemplateCollectionIds;

        return new(
            spec,
            aggregator.Templates.GetBuildings(ids),
            aggregator.Templates.GetPlants(ids),
            
        );
    }
}
