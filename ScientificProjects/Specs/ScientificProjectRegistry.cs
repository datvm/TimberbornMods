
namespace ScientificProjects.Specs;

public class ScientificProjectRegistry(ISpecService specs, IEnumerable<IProjectCostProvider> costProviders, ILoc t) : ILoadableSingleton
{
    FrozenDictionary<string, ScientificProjectGroupSpec> groupsById = null!;
    FrozenDictionary<string, List<ScientificProjectSpec>> groupProjects = null!;
    ImmutableArray<ScientificProjectGroupSpec> allGroups = [];

    internal FrozenDictionary<string, ScientificProjectSpec> projectsById = null!;
    ILookup<string, ScientificProjectSpec> projectsUnlockedById = null!;
    ImmutableArray<ScientificProjectSpec> allProjects = [];

    FrozenDictionary<string, IProjectCostProvider> costProvidersById = null!;

    public IEnumerable<ScientificProjectGroupSpec> AllGroups => allGroups;
    public IEnumerable<ScientificProjectSpec> AllProjects => allProjects;

    public ScientificProjectSpec GetProject(string id) => projectsById[id];
    public ScientificProjectGroupSpec GetGroup(string id) => groupsById[id];
    public IProjectCostProvider GetCostProviderFor(string id) => costProvidersById[id];

    public IEnumerable<ScientificProjectSpec> GetProjects(string groupId) => groupProjects[groupId];

    public void Load()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        LoadGroups();
        
        LoadProjects();
        CheckForCircularRequirement();

        LoadCostProviders();

        sw.Stop();
        Debug.Log($"Loaded {allProjects.Length} projects in {sw.ElapsedMilliseconds}ms");
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
    void LoadProjects()
    {
        var projects = specs.GetSpecs<ScientificProjectSpec>();

        foreach (var p in projects)
        {
            if (p.Id.Contains(';'))
            {
                throw new InvalidOperationException($"Project Id must not have a semi-colon: {p.Id}");
            }

            p.DisplayName = t.T(p.NameKey);
            p.Effect = string.Format(t.T(p.EffectKey), [.. p.Parameters]);

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
