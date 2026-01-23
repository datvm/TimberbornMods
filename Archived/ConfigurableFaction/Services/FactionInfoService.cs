namespace ConfigurableFaction.Services;

public class FactionInfoService(
    ISpecService specs,
    ConfigurableFactionSpecService factionSpecService,
    IAssetLoader assets
)
{
    public const string CommonGroup = "Common";
    public static readonly ImmutableHashSet<string> SkipGroups = [CommonGroup];

    public FactionsInfo? FactionsInfo { get; private set; }

#nullable disable
    public FrozenDictionary<string, NeedSpec> Needs { get; private set; }
    public FrozenDictionary<string, GoodSpec> Goods { get; private set; }
    public FrozenDictionary<string, PrefabGroupSpec> PrefabGroups { get; private set; }
    public FrozenDictionary<string, ToolGroupSpec> ToolGroups { get; private set; }
    public FrozenDictionary<string, RecipeSpec> Recipes { get; private set; }
    public FrozenDictionary<string, NormalizedPrefabSpec> PrefabsByPaths { get; private set; }
#nullable enable

    Dictionary<string, NormalizedPrefabSpec> prefabsByPaths = [];


    public void ScanFactions(bool force = false)
    {
        if (!force && FactionsInfo is not null) { return; }

        var factions = factionSpecService.OriginalFactions;

        Needs = specs.GetSpecs<NeedSpec>(q => q.Id);
        Goods = specs.GetSpecs<GoodSpec>(q => q.Id);
        PrefabGroups = GroupPrefabGroups();
        ToolGroups = specs.GetSpecs<ToolGroupSpec>(q => q.Id);
        Recipes = specs.GetSpecs<RecipeSpec>(q => q.Id);
        prefabsByPaths = [];

        FactionsInfo = new()
        {
            Factions = [.. factions.Select(ScanFaction)]
        };

        PrefabsByPaths = prefabsByPaths.ToFrozenDictionary();
        prefabsByPaths = [];
    }

    FrozenDictionary<string, PrefabGroupSpec> GroupPrefabGroups()
    {
        Dictionary<string, List<string>> all = [];
        var grps = specs.GetSpecs<PrefabGroupSpec>();

        foreach (var grp in grps)
        {
            var id = grp.Id;
            if (!all.TryGetValue(id, out var curr))
            {
                all[id] = curr = [];
            }

            if (!grp.Paths.IsDefaultOrEmpty)
            {
                curr.AddRange(grp.Paths);
            }
        }

        return all.ToFrozenDictionary(q => q.Key, q => new PrefabGroupSpec()
        {
            Id = q.Key,
            Paths = [.. q.Value],
        });
    }

    FactionInfo ScanFaction(FactionSpec spec)
    {
        var prefabGroups = spec.PrefabGroups.Map(PrefabGroups!, true);

        List<NormalizedPrefabSpec> buildings = [];
        Dictionary<string, List<NormalizedPrefabSpec>> plantables = [];

        foreach (var grp in prefabGroups)
        {
            if (grp.IsModPrefabGroup() || SkipGroups.Contains(grp.Id)) { continue; }

            foreach (var path in grp.Paths)
            {
                var obj = assets.Load<GameObject>(path);
                var prefab = obj.GetComponent<PrefabSpec>();
                if (!prefab) { continue; }

                var normalized = NormalizedPrefabSpec.Create(prefab, path, spec.Id);
                prefabsByPaths[normalized.Path] = normalized;

                // Check for Plants
                var plant = prefab.GetComponentFast<PlantableSpec>();
                if (plant)
                {
                    var cat = plant.ResourceGroup;
                    if (!plantables.TryGetValue(cat, out var list))
                    {
                        plantables[cat] = list = [];
                    }

                    list.Add(normalized);
                }
                else // Check for buildings
                {
                    var building = prefab.GetComponentFast<PlaceableBlockObjectSpec>();
                    if (building)
                    {
                        buildings.Add(normalized);
                    }
                }
            }
        }

        return new(
            spec,
            spec.Needs.Map(Needs),
            spec.Goods.Map(Goods),
            prefabGroups,
            [.. buildings],
            [.. plantables.Select(kv => new PlantableGroup(kv.Key, [.. kv.Value]))],
            GroupBuildings(buildings)
        );
    }

    ImmutableArray<BuildingToolGroup> GroupBuildings(IEnumerable<NormalizedPrefabSpec> buildings) => [.. buildings
        .Select(q =>
        {
            var objSpec = q.PrefabSpec.GetComponentFast<PlaceableBlockObjectSpec>();
            var toolGroup = ToolGroups[objSpec.ToolGroupId];

            return (q, objSpec, toolGroup);
        })
        .GroupBy(q => q.toolGroup, (k, list) => new BuildingToolGroup(
            k,
            [.. list
                .OrderBy(q => q.objSpec.ToolOrder)
                .Select(q => q.q)]
        )).OrderBy(q => q.ToolGroupSpec.Order)
    ];

}
