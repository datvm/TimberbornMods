namespace ConfigurableFaction.Models;

public class AggregatedTemplateCollection : AggregatedCollectionBase<TemplateCollectionSpec, TemplateSpec, TemplateCollectionDef, TemplateDefBase>
{
    public static readonly AggregatedTemplateCollection Empty = new();

    readonly DataAggregatorService aggregator = null!;
    readonly TemplateDefFactory fac = null!;

    public FrozenDictionary<string, ImmutableArray<string>> IdsByTemplateNames { get; private set; } = FrozenDictionary<string, ImmutableArray<string>>.Empty;

    AggregatedTemplateCollection() : base() { }
    public AggregatedTemplateCollection(ISpecService specs, DataAggregatorService dataAggregatorService, TemplateDefFactory fac) : base(specs)
    {
        aggregator = dataAggregatorService;
        this.fac = fac;
    }

    public IEnumerable<BuildingDef> AllBuildings => ItemsByIds.Values.OfType<BuildingDef>();
    public IEnumerable<PlantDef> AllPlants => ItemsByIds.Values.OfType<PlantDef>();

    public IEnumerable<BuildingDef> GetBuildings(IEnumerable<string> collectionIds)
        => GetByCollectionIds(collectionIds).OfType<BuildingDef>().OrderBy(q => q.Order);

    public IEnumerable<PlantDef> GetPlants(IEnumerable<string> collectionIds)
        => GetByCollectionIds(collectionIds).OfType<PlantDef>().OrderBy(q => q.Order);

    public HashSet<string> CommonBuildingsIds => [.. CommonItems.OfType<BuildingDef>().Select(b => b.Id)];
    public HashSet<string> CommonPlantsIds => [.. CommonItems.OfType<PlantDef>().Select(p => p.Id)];

    public override void Aggregate()
    {
        var activeList = aggregator.ActiveTemplateCollections.CollectionIds;

        Dictionary<string, HashSet<TemplateDefBase>> templatesGroups = [];
        Dictionary<string, TemplateDefBase> templatesByIds = [];

        foreach (var colSpec in specs.GetSpecs<TemplateCollectionSpec>())
        {
            var colId = colSpec.CollectionId;
            if (!activeList.Contains(colId)) { continue; }

            var grp = templatesGroups.GetOrAdd(colId);
            foreach (var bpRef in colSpec.Blueprints)
            {
                var path = bpRef.Path; // Also acts as ID
                if (!templatesByIds.TryGetValue(path, out var template))
                {
                    template = fac.Create(bpRef, aggregator);
                    if (template is null) { continue; }

                    templatesByIds[path] = template;
                }

                grp.Add(template);
            }
        }

        ItemsByIds = templatesByIds.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        CollectionsByIds = templatesGroups.ToFrozenDictionary(
            kv => kv.Key,
            kv => new TemplateCollectionDef(kv.Key, [.. kv.Value]));

        IdsByTemplateNames = ItemsByIds.Values
            .GroupBy(t => t.TemplateName)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.Select(t => t.Id).ToImmutableArray());
    }

    // Custom logic no longer call these methods:
    public override string? GetCollectionId(TemplateCollectionSpec spec) => throw new NotImplementedException();
    public override IEnumerable<TemplateDefBase> GetItems(TemplateCollectionSpec spec) => throw new NotImplementedException();
    public override string GetItemSpecId(TemplateSpec item) => throw new NotImplementedException();
    public override string GetItemId(TemplateDefBase item) => throw new NotImplementedException();
    public override TemplateCollectionDef CreateCollection(string id, List<TemplateDefBase> items) => throw new NotImplementedException();
}

public record TemplateCollectionDef(string Id, ImmutableArray<TemplateDefBase> Templates) : CollectionDefBase<TemplateDefBase>(Id, Templates)
{
    public IEnumerable<BuildingDef> Buildings => Templates.OfType<BuildingDef>();
    public IEnumerable<PlantDef> Plants => Templates.OfType<PlantDef>();
}
