namespace ConfigurableFaction.Models;

public class AggregatedTemplateCollection : AggregatedCollectionBase<TemplateCollectionSpec, TemplateSpec, TemplateCollectionDef, TemplateDefBase>
{
    public static readonly AggregatedTemplateCollection Empty = new();

    readonly DataAggregatorService aggregator = null!;
    readonly TemplateDefFactory fac = null!;

    public FrozenDictionary<string, string> IdsByTemplateNames { get; private set; } = FrozenDictionary<string, string>.Empty;
    
    AggregatedTemplateCollection() : base() { }
    public AggregatedTemplateCollection(ISpecService specs, DataAggregatorService dataAggregatorService, TemplateDefFactory fac) : base(specs)
    {
        aggregator = dataAggregatorService;
        this.fac = fac;
    }

    public override TemplateCollectionDef CreateCollection(string id, List<TemplateDefBase> items) => new(id, [.. items]);

    public override string? GetCollectionId(TemplateCollectionSpec spec) => spec.CollectionId;

    public override string GetItemId(TemplateDefBase item) => item.Id;

    public IEnumerable<BuildingDef> AllBuildings => ItemsByIds.Values.OfType<BuildingDef>();
    public IEnumerable<PlantDef> AllPlants => ItemsByIds.Values.OfType<PlantDef>();

    public IEnumerable<BuildingDef> GetBuildings(IEnumerable<string> collectionIds)
        => GetByCollectionIds(collectionIds).OfType<BuildingDef>().OrderBy(q => q.Order);

    public IEnumerable<PlantDef> GetPlants(IEnumerable<string> collectionIds)
        => GetByCollectionIds(collectionIds).OfType<PlantDef>().OrderBy(q => q.Order);

    public HashSet<string> CommonBuildingsIds => [.. CommonItems.OfType<BuildingDef>().Select(b => b.Id)];
    public HashSet<string> CommonPlantsIds => [.. CommonItems.OfType<PlantDef>().Select(p => p.Id)];

    public override IEnumerable<TemplateDefBase> GetItems(TemplateCollectionSpec spec) =>
        spec.Blueprints
            .Select(bp => fac.Create(bp, aggregator))
            .Where(d => d is not null)!;

    public override string GetItemSpecId(TemplateSpec item) => item.TemplateName;

    public override void Aggregate()
    {
        base.Aggregate();

        IdsByTemplateNames = ItemsByIds.Values.ToFrozenDictionary(t => t.TemplateName, t => t.Id);
    }
}

public record TemplateCollectionDef(string Id, ImmutableArray<TemplateDefBase> Templates) : CollectionDefBase<TemplateDefBase>(Id, Templates)
{
    public IEnumerable<BuildingDef> Buildings => Templates.OfType<BuildingDef>();
    public IEnumerable<PlantDef> Plants => Templates.OfType<PlantDef>();
}
