namespace ConfigurableFaction.Definitions;

public class AggregatedTemplateCollection : AggregatedCollectionBase<TemplateCollectionSpec, TemplateSpec, TemplateCollectionDef, TemplateDefBase>
{

    public static readonly AggregatedTemplateCollection Empty = new();

    readonly ISpecService specs;
    readonly DataAggregatorService dataAggregatorService;
    readonly ILoc t;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    AggregatedTemplateCollection() : base() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public AggregatedTemplateCollection(ISpecService specs, DataAggregatorService dataAggregatorService, ILoc t) : base(specs)
    {
        this.dataAggregatorService = dataAggregatorService;
        this.t = t;
        this.specs = specs;
    }

    public override TemplateCollectionDef CreateCollection(string id, List<TemplateDefBase> items) => new(id, [.. items]);

    public override string GetCollectionId(TemplateCollectionSpec spec) => spec.CollectionId;
    public override string GetItemId(TemplateDefBase item) => item.TemplateName;

    public IEnumerable<BuildingDef> GetBuildings(IEnumerable<string> collectionIds)
        => GetByCollectionIds(collectionIds).OfType<BuildingDef>();
    public IEnumerable<PlantDef> GetPlants(IEnumerable<string> collectionIds)
        => GetByCollectionIds(collectionIds).OfType<PlantDef>();

    public override IEnumerable<TemplateDefBase> GetItems(TemplateCollectionSpec spec)
    {
        foreach (var bpAsset in spec.Blueprints)
        {
            var bp = specs.GetBlueprint(bpAsset.Path);

            var building = BuildingDef.Create(bp, dataAggregatorService, t);
            if (building is not null)
            {
                yield return building;
                continue;
            }

            var plant = PlantDef.Create(bp, dataAggregatorService, t);
            if (plant is not null)
            {
                yield return plant;
                continue;
            }
        }
    }

    public override string GetItemSpecId(TemplateSpec item) => item.TemplateName;
}

public record TemplateCollectionDef(string Id, ImmutableArray<TemplateDefBase> Templates) : CollectionDefBase<TemplateDefBase>(Id, Templates)
{
    public IEnumerable<BuildingDef> Buildings => Templates.OfType<BuildingDef>();
    public IEnumerable<PlantDef> Plants => Templates.OfType<PlantDef>();
}
