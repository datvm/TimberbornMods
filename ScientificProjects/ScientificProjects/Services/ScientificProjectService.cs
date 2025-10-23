namespace ScientificProjects.Services;

public class ScientificProjectService(
    ScientificProjectRegistry registry,
    ScientificProjectDailyService daily,
    ScientificProjectUnlockService unlocks,
    FactionService factionService
) : ILoadableSingleton
{
    readonly Dictionary<string, ScientificProjectInfo> activeProjects = [];
    public IReadOnlyDictionary<string, ScientificProjectInfo> ActiveProjects => activeProjects;

    public void Load()
    {
        daily.OnDailyPaymentResolved += OnDailyPaymentResolved;
        unlocks.OnProjectUnlocked += OnProjectUnlocked;

        OnDailyPaymentResolved();
    }

    void OnProjectUnlocked(ScientificProjectSpec project) => activeProjects[project.Id] = GetProject(project);

    void OnDailyPaymentResolved()
    {
        activeProjects.Clear();

        foreach (var id in unlocks.UnlockedProjectIds)
        {
            var p = GetProject(id);
            
            if (p.Active)
            {
                activeProjects[id] = p;
            }
        }
    }

    public ScientificProjectInfo GetProject(string id) => GetProject(registry.GetProject(id));
    public ScientificProjectInfo GetProject(ScientificProjectSpec spec)
    {
        var levels = daily.GetLevels(spec);
        var unlocked = unlocks.IsUnlocked(spec.Id);

        var required = spec.RequiredId is null ? null : registry.GetProject(spec.RequiredId);
        var requirementUnlocked = required is null || unlocks.IsUnlocked(required.Id);

        return new(spec, unlocked, levels, required, requirementUnlocked);
    }

    public ImmutableArray<ScientificProjectInfo> GetGroupProjects(string groupId)
    {
        var factionId = factionService.Current.Id;

        return [.. registry
            .GroupProjects[groupId]
            .Where(q => q.IsAvailableTo(factionId))
            .Select(GetProject)];
    }

    public IEnumerable<ScientificProjectInfo> GetActiveProjects(IEnumerable<string> ids)
    {
        foreach (var id in ids)
        {
            if (activeProjects.TryGetValue(id, out var info))
            {
                yield return info;
            }
        }
    }

    public float GetActiveEffects(IEnumerable<string> ids, int parameterIndex)
    {
        var total = 0f;
        foreach (var id in ids)
        {
            if (activeProjects.TryGetValue(id, out var info))
            {
                total += info.GetEffect(parameterIndex);
            }
        }
        return total;
    }

}
