
namespace ScientificProjects.Specs;

public class ScientificProjectRegistry(
    ISpecService specs,
    IEnumerable<IProjectCostProvider> costProviders,
    IEnumerable<IProjectUnlockConditionProvider> unlockConditionProviders,
    ILoc t
) : ILoadableSingleton
{
    FrozenDictionary<string, ScientificProjectGroupSpec> groupsById = null!;
    FrozenDictionary<string, List<ScientificProjectSpec>> groupProjects = null!;
    ImmutableArray<ScientificProjectGroupSpec> allGroups = [];

    FrozenDictionary<string, ScientificProjectSpec> projectsById = null!;
    ILookup<string, ScientificProjectSpec> projectsUnlockedById = null!;
    ImmutableArray<ScientificProjectSpec> allProjects = [];

    FrozenDictionary<string, IProjectCostProvider> costProvidersById = null!;
    FrozenDictionary<string, IProjectUnlockConditionProvider> unlockConditionProvidersById = null!;

    public IEnumerable<ScientificProjectGroupSpec> AllGroups => allGroups;
    public IEnumerable<ScientificProjectSpec> AllProjects => allProjects;

    public bool TryGetProject(string id, [MaybeNullWhen(false)] out ScientificProjectSpec? project) => projectsById.TryGetValue(id, out project);
    public ScientificProjectSpec GetProject(string id) => projectsById[id];
    public ScientificProjectGroupSpec GetGroup(string id) => groupsById[id];
    public IProjectCostProvider GetCostProviderFor(string id) => costProvidersById[id];
    public IProjectUnlockConditionProvider GetUnlockConditionProviderFor(string id) => unlockConditionProvidersById[id];

    public IEnumerable<ScientificProjectSpec> GetProjects(string groupId) => groupProjects[groupId];
    public IEnumerable<ScientificProjectSpec> GetProjects(string groupId, string? faction)
    {
        var projects = GetProjects(groupId);
        
        return faction is null ? projects : projects.Where(q => q.IsAvailableTo(faction));
    }

    public void Load()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        LoadGroups();

        LoadProjects();
        CheckForCircularRequirement();

        LoadCostProviders();
        LoadUnlockConditionProviders();

        sw.Stop();
    }

    void LoadGroups()
    {
        var groups = specs.GetSpecs<ScientificProjectGroupSpec>();
        foreach (var g in groups)
        {
            g.DisplayName = t.T(g.NameKey);
            g.Description = t.T(g.DescKey);
        }

        allGroups = [.. groups.OrderBy(g => g.Order)];
        groupsById = allGroups.ToFrozenDictionary(g => g.Id);
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

            if (!groupsById.ContainsKey(p.GroupId))
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

        allProjects = [.. projects.OrderBy(q => q.Order)];

        projectsById = allProjects.ToFrozenDictionary(p => p.Id);
        projectsUnlockedById = allProjects
            .Where(p => p.RequiredId is not null)
            .ToLookup(p => p.RequiredId!);

        groupProjects = allProjects.GroupBy(q => q.GroupId)
            .ToFrozenDictionary(
                q => q.Key,
                q => q.OrderBy(q => q.Order).ToList());
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

        costProvidersById = providers.ToFrozenDictionary();
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

        unlockConditionProvidersById = providers.ToFrozenDictionary();
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
