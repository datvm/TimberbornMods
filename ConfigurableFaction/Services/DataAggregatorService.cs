namespace ConfigurableFaction.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class DataAggregatorService(
    ILoc t,
    ISpecService specs
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
    public FrozenDictionary<string, RecipeSpec> RecipesByIds { get; private set; } = FrozenDictionary<string, RecipeSpec>.Empty;
    public AggregatedTemplateCollection Templates { get; private set; } = AggregatedTemplateCollection.Empty;
    public AggregatedFactionCollection Factions { get; private set; } = AggregatedFactionCollection.Empty;
    public ImmutableArray<ExclusiveGroupDef> ExclusiveGroups { get; private set; } = [];

    bool initialized;

    public void Initialize()
    {
        if (initialized) { return; }
        initialized = true;

        RecipesByIds = specs.GetSpecs<RecipeSpec>().ToFrozenDictionary(r => r.Id);
        Needs = new(specs);
        Goods = new(specs);
        Templates = new(specs, this, t);
        Factions = new(specs, this);

        PopulateExclusiveGroups();
    }

    void PopulateExclusiveGroups()
    {
        Dictionary<string, ExclusiveGroupDef> groups = [];

        foreach (var b in Templates.AllBuildings)
        {
            var planter = b.Blueprint.GetSpec<PlanterBuildingSpec>();
            if (planter is not null)
            {
                var groupName = $"{nameof(PlanterBuildingSpec)}.{planter.PlantableResourceGroup}";
                groups.GetOrAdd(groupName, () => new(groupName)).Templates.Add(b.TemplateName);
            }
        }

        ExclusiveGroups = [.. groups.Values];
    }

}
