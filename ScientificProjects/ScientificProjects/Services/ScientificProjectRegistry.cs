namespace ScientificProjects.Services;

public class ScientificProjectRegistry(
    ISpecService specs,
    IEnumerable<IProjectCostProvider> costProviders,
    IEnumerable<IProjectUnlockConditionProvider> unlockConditionProviders,
    ILoc t,
    IEnumerable<IScientificProjectRegistryModifier> modifiers
) : ILoadableSingleton
{
    public FrozenDictionary<string, ScientificProjectGroupSpec> GroupsById { get; private set; } = null!;
    public FrozenDictionary<string, ImmutableArray<ScientificProjectSpec>> GroupProjects { get; private set; } = null!;
    public ImmutableArray<ScientificProjectGroupSpec> AllGroups { get; private set; } = [];

    public FrozenDictionary<string, ScientificProjectSpec> ProjectsById { get; private set; } = null!;
    public ILookup<string, ScientificProjectSpec> ProjectsUnlockedById { get; private set; } = null!;
    public ImmutableArray<ScientificProjectSpec> AllProjects { get; private set; } = [];

    public FrozenDictionary<string, IProjectCostProvider> CostProvidersById { get; private set; } = null!;
    public FrozenDictionary<string, IProjectUnlockConditionProvider> UnlockConditionProvidersById { get; private set; } = null!;

    public ScientificProjectSpec GetProject(string id) => ProjectsById[id];
    public ScientificProjectGroupSpec GetGroup(string id) => GroupsById[id];
    public IProjectCostProvider GetCostProviderFor(string id) => CostProvidersById[id];
    public IProjectUnlockConditionProvider GetUnlockConditionProviderFor(string id) => UnlockConditionProvidersById[id];

    public IEnumerable<ScientificProjectSpec> GetProjects(string groupId) => GroupProjects[groupId];
    public IEnumerable<ScientificProjectSpec> GetProjects(string groupId, string? faction)
    {
        var projects = GetProjects(groupId);
        
        return faction is null ? projects : projects.Where(q => q.IsAvailableTo(faction));
    }

    public void Load()
    {
        LoadGroups();

        LoadProjects();
        CheckForCircularRequirement();

        LoadCostProviders();
        LoadUnlockConditionProviders();

        RunModifiers();
    }

    void RunModifiers()
    {
        foreach (var m in modifiers)
        {
            m.Modify(this);
        }
    }

    void LoadGroups()
    {
        var groups = specs.GetSpecs<ScientificProjectGroupSpec>();
        foreach (var g in groups)
        {
            g.DisplayName = t.T(g.NameKey);
            g.Description = t.T(g.DescKey);
        }

        AllGroups = [.. groups.OrderBy(g => g.Order)];
        GroupsById = AllGroups.ToFrozenDictionary(g => g.Id);
    }

    readonly HashSet<string> expectingCostProviderIds = [];
    readonly HashSet<string> expectingUnlockConditionProviderIds = [];
    void LoadProjects()
    {
        var projects = specs.GetSpecs<ScientificProjectSpec>();

        foreach (var p in projects)
        {
            if (p.Id.Contains(';'))
            {
                throw new InvalidOperationException($"Project Id must not have a semi-colon: {p.Id}");
            }

            if (!GroupsById.ContainsKey(p.GroupId))
            {
                throw new InvalidOperationException($"Project {p.Id} has undefined {nameof(p.GroupId)}: {p.GroupId}");
            }

            p.DisplayName = t.T(p.NameKey);

            p.Effect = string.Format(t.T(p.EffectKey), [.. p.Parameters]);

            if (p.LoreKey is not null)
            {
                p.Lore = t.T(p.LoreKey);
            }

            if (p.HasScalingCost)
            {
                if (string.IsNullOrEmpty(p.ScalingCostKey))
                {
                    throw new InvalidDataException($"Project {p.Id} has {nameof(p.ScalingCostKey)} == true but no {p.ScalingCostKey} is set.");
                }

                p.ScalingCostDisplay = string
                    .Format(t.T(p.ScalingCostKey), [.. p.Parameters])
                    .Replace("[Cost]", NumberFormatter.Format(p.ScienceCost));

                expectingCostProviderIds.Add(p.Id);
            }

            if (p.HasCustomUnlockCondition)
            {
                if (p.HasSteps)
                {
                    throw new InvalidDataException($"Projects with Steps ({nameof(p.MaxSteps)} > 0) does not support custom unlock conditions. Violated project Id: {p.Id}");
                }

                expectingUnlockConditionProviderIds.Add(p.Id);
            }
        }

        AllProjects = [.. projects.OrderBy(q => q.Order)];

        ProjectsById = AllProjects.ToFrozenDictionary(p => p.Id);
        ProjectsUnlockedById = AllProjects
            .Where(p => p.RequiredId is not null)
            .ToLookup(p => p.RequiredId!);

        var groups = AllProjects.GroupBy(q => q.GroupId)
            .ToDictionary(
                q => q.Key,
                q => q.OrderBy(q => q.Order).ToImmutableArray());

        // For empty groups
        foreach (var g in AllGroups)
        {
            if (!groups.ContainsKey(g.Id))
            {
                groups[g.Id] = [];
            }
        }
        GroupProjects = groups.ToFrozenDictionary();
    }

    void LoadCostProviders()
    {
        Dictionary<string, IProjectCostProvider> providers = [];
        foreach (var p in costProviders)
        {
            var ids = p.CanCalculateCostForIds;
            foreach (var id in ids)
            {
                providers[id] = p;
                expectingCostProviderIds.Remove(id);
            }
        }

        if (expectingCostProviderIds.Count > 0)
        {
            throw new InvalidOperationException($"Missing cost providers for: {string.Join(", ", expectingCostProviderIds)}");
        }

        CostProvidersById = providers.ToFrozenDictionary();
    }

    void LoadUnlockConditionProviders()
    {
        Dictionary<string, IProjectUnlockConditionProvider> providers = [];
        foreach (var p in unlockConditionProviders)
        {
            var ids = p.CanCheckUnlockConditionForIds;
            foreach (var id in ids)
            {
                providers[id] = p;
                expectingUnlockConditionProviderIds.Remove(id);
            }
        }

        if (expectingUnlockConditionProviderIds.Count > 0)
        {
            throw new InvalidOperationException($"Missing unlock condition providers for: {string.Join(", ", expectingUnlockConditionProviderIds)}");
        }

        UnlockConditionProvidersById = providers.ToFrozenDictionary();
    }

    void CheckForCircularRequirement()
    {
        HashSet<string> items = [];

        foreach (var p in AllProjects)
        {
            if (items.Contains(p.Id)) { continue; }
            items.Add(p.Id);

            List<string> currCycle = [p.Id];

            var curr = p.RequiredId;
            while (curr is not null)
            {
                var required = GetProject(curr);
                currCycle.Add(curr);

                items.Add(required.Id);
                curr = required.RequiredId;

                if (curr is null) { break; }

                if (currCycle.Contains(curr))
                {
                    throw new InvalidDataException($"Circular Project requirement detected: {string.Join(" -> ", currCycle)} -> {curr}");
                }

            }
        }
    }

}
