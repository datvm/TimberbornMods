namespace ConfigurableFaction.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class DataAggregatorService(
    ILoc t,
    ISpecService specs,
    FactionSpecService factionSpecService
)
{
    public const string CommonId = "Common";
    public static readonly FrozenDictionary<string, string> SpecialPairBuildings = new KeyValuePair<string, string>[]
        {
            new("Buildings/Monuments/EarthRepopulator/EarthRepopulator.IronTeeth", "Buildings/Monuments/EarthRepopulator/EarthRepopulator.IronTeeth.Plane" ),
        }
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public AggregatedNeedCollection Needs { get; private set; } = AggregatedNeedCollection.Empty;
    public AggregatedGoodCollection Goods { get; private set; } = AggregatedGoodCollection.Empty;
    public AggregatedTemplateCollection Templates { get; private set; } = AggregatedTemplateCollection.Empty;
    public AggregatedFactionCollection Factions { get; private set; } = AggregatedFactionCollection.Empty;
    public ImmutableArray<ExclusiveGroupDef> ExclusiveGroups { get; private set; } = [];

    bool initialized;

    public void Initialize()
    {
        if (initialized) { return; }
        initialized = true;

        Needs = new(specs);
        Goods = new(specs);
        Templates = new(specs, this, t);
        Factions = new(specs, this);

        LoadTemplates();
        PopulateFactionTemplates();
        PopulateExclusiveGroups();
        PopulateFactionNeeds();

        ExclusiveGroups = [.. exclusiveGroups];

        Cleanup();
    }

    void LoadFactions()
    {
        Factions = [.. factionSpecService.Factions
            .Select(f => new FactionDef(f))];
        FactionById = Factions.ToFrozenDictionary(f => f.Id);
    }

    void LoadTemplates()
    {
        var blueprintGroups = GetCollectionItems<TemplateCollectionSpec, AssetRef<BlueprintAsset>>(
            s => s.CollectionId, s => s.Blueprints);

        Dictionary<string, TemplateCollectionContent> groups = [];

        foreach (var (id, blueprints) in blueprintGroups)
        {
            if (id == CommonId) { continue; }
            var group = groups[id] = new();

            foreach (var bpAsset in blueprints)
            {
                var bp = specs.GetBlueprint(bpAsset.Path);

                var buildingDef = BuildingDef.Create(bp, this, t);
                if (buildingDef is not null)
                {
                    group.Buildings.Add(buildingDef);
                }
                else
                {
                    var plantDef = PlantDef.Create(bp, this, t);
                    if (plantDef is not null)
                    {
                        group.Plants.Add(plantDef);
                    }
                }
            }
        }

        templatesByCollectionIds = groups.ToFrozenDictionary();
        BuildingsByTemplateNames = templatesByCollectionIds
            .Values
            .SelectMany(b => b.Buildings)
            .ToFrozenDictionary(b => b.TemplateName);
    }

    void PopulateExclusiveGroups()
    {
        Dictionary<string, ExclusiveGroupDef> groups = [];

        foreach (var b in BuildingsByTemplateNames.Values)
        {
            var planter = b.Blueprint.GetSpec<PlanterBuildingSpec>();
            if (planter is not null)
            {
                var groupName = $"{nameof(PlanterBuildingSpec)}.{planter.PlantableResourceGroup}";
                groups.GetOrAdd(groupName, () => new(groupName)).Templates.Add(b.TemplateName);
            }
        }

        exclusiveGroups = [.. groups.Values];
    }

    void PopulateFactionTemplates()
    {
        foreach (var f in Factions)
        {
            TemplateCollectionContent fTemplates = new();

            foreach (var col in f.Spec.TemplateCollectionIds)
            {
                if (!templatesByCollectionIds.TryGetValue(col, out var templates)) { continue; }
                fTemplates.AddRange(templates);
            }

            f.Buildings = [.. fTemplates.Buildings.OrderBy(b => b.DisplayName)];
            f.BuildingsByGroups = f.Buildings
                .GroupBy(b => b.GroupId)
                .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());

            f.Plants = [.. fTemplates.Plants.OrderBy(p => p.DisplayName)];
        }
    }

    void PopulateFactionNeeds()
    {

    }

    Dictionary<string, List<TItem>> GetCollectionItems<T, TItem>(Func<T, string> getId, Func<T, IEnumerable<TItem>> getItems) 
        where T : ComponentSpec
    {
        Dictionary<string, List<TItem>> result = [];

        foreach (var col in specs.GetSpecs<T>())
        {
            var id = getId(col);
            var list = result.GetOrAdd(id);
            list.AddRange(getItems(col));
        }

        return result;
    }

    void Cleanup()
    {
        templatesByCollectionIds = null!;
        exclusiveGroups = null!;
    }

}
