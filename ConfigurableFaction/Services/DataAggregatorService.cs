namespace ConfigurableFaction.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class DataAggregatorService(
    ISpecService specs,
    TemplateDefFactory templateDefFactory
)
{

    public AggregatedNeedCollection Needs { get; private set; } = AggregatedNeedCollection.Empty;
    public AggregatedGoodCollection Goods { get; private set; } = AggregatedGoodCollection.Empty;
    public FrozenDictionary<string, RecipeSpec> RecipesByIds { get; private set; } = FrozenDictionary<string, RecipeSpec>.Empty;
    public FrozenDictionary<string, BlockObjectToolGroupSpec> ToolGroupsByIds { get; private set; } = FrozenDictionary<string, BlockObjectToolGroupSpec>.Empty;
    public AggregatedTemplateCollection Templates { get; private set; } = AggregatedTemplateCollection.Empty;
    public AggregatedFactionCollection Factions { get; private set; } = AggregatedFactionCollection.Empty;
    public ImmutableArray<ExclusiveGroupDef> ExclusiveGroups { get; private set; } = [];

    bool initialized;

    public void Initialize()
    {
        if (initialized) { return; }
        initialized = true;

        RecipesByIds = specs.GetSpecs<RecipeSpec>().ToFrozenDictionary(r => r.Id);
        ToolGroupsByIds = specs.GetSpecs<BlockObjectToolGroupSpec>().ToFrozenDictionary(t => t.Id);
        Needs = new(specs); Needs.Aggregate();
        Goods = new(specs); Goods.Aggregate();
        Templates = new(specs, this, templateDefFactory); Templates.Aggregate();
        Factions = new(specs, this); Factions.Aggregate();

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
                groups.GetOrAdd(groupName, () => new(groupName)).Templates.Add(b.Id);
            }
        }

        ExclusiveGroups = [.. groups.Values];
    }

}
