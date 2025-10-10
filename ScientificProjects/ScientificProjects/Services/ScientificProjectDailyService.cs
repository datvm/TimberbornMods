namespace ScientificProjects.Services;

public class ScientificProjectDailyService(
    ISingletonLoader loader,
    ScientificProjectRegistry registry,
    ScientificProjectUnlockService unlocksService,
    EventBus eb,
    ScienceService sciences
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly ListKey<string> ProjectLevelsKey = new("ProjectLevels");
    static readonly ListKey<string> TodayProjectLevelsKey = new("TodayProjectLevels");
    static readonly PropertyKey<int> YesterdayScienceKey = new("YesterdayScience");
    static readonly PropertyKey<int> ScienceGainedKey = new("ScienceGained");
    static readonly PropertyKey<bool> HasYesterdayScienceKey = new("HasYesterdayScience");

    readonly Dictionary<string, int> levels = [];
    readonly Dictionary<string, int> todayLevels = [];

    public IReadOnlyDictionary<string, int> TodayLevels => todayLevels;
    public IReadOnlyDictionary<string, int> Levels => levels;

    bool hasYesterdayScience = false;
    int yesterdayScience = -1;
    public int? ScienceGainedToday { get; private set; }

    public void SetLevel(ScientificProjectSpec project, int level)
    {
        if (project.MaxSteps < level)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, $"Project {project.Id} has {nameof(project.MaxSteps)} = {project.MaxSteps}");
        }

        levels[project.Id] = level;
    }

    public ScientificProjectLevels GetLevels(ScientificProjectSpec project)
        => project.HasSteps
            ? GetLevels(project.Id)
            : ScientificProjectLevels.NoLevel;

    public ScientificProjectLevels GetLevels(string id)
        => new(
            Today: todayLevels.TryGetValue(id, out var t) ? t : 0,
            NextDay: levels.TryGetValue(id, out var l) ? l : 0
        );

    public event Action? OnDailyPaymentResolved;

    [OnEvent]
    public void OnDayStarted(DaytimeStartEvent _)
    {
        UpdateHistoricScience();
        TryPayingDailyScience();
    }

    void TryPayingDailyScience()
    {
        var cost = CalculateDayCost();
        var have = sciences.SciencePoints;

        if (cost > have)
        {
            eb.PostNow(new OnScientificProjectDailyNotEnoughEvent(cost, have));
        }
        else
        {
            PayForToday(cost);
        }
    }

    void UpdateHistoricScience()
    {
        if (hasYesterdayScience || yesterdayScience != -1)
        {
            hasYesterdayScience = true;
            ScienceGainedToday = sciences.SciencePoints - yesterdayScience;
            yesterdayScience = sciences.SciencePoints;
        }
        else
        {
            yesterdayScience = sciences.SciencePoints;
        }
    }

    public void SkipTodayPayment() => SetTodayScience(0, true);
    public void PayForToday() => PayForToday(CalculateDayCost());
    public void PayForToday(int total) => SetTodayScience(total, false);

    void SetTodayScience(int cost, bool skipped)
    {
        sciences.SubtractPoints(cost);
        todayLevels.Clear();

        if (!skipped)
        {
            foreach (var (id, level) in levels)
            {
                if (level == 0) { continue; }

                todayLevels[id] = level;
            }
        }

        OnDailyPaymentResolved?.Invoke();
        eb.Post(new OnScientificProjectDailyCostChargedEvent(cost));
    }

    public void Load()
    {
        LoadSavedData();
        eb.Register(this);
    }

    public int CalculateDayCost()
    {
        var total = 0;

        foreach (var (id, level) in levels)
        {
            if (level == 0) { continue; }

            var spec = registry.GetProject(id);
            var cost = GetDailyCost(spec);
            total += cost;
        }

        return total;
    }

    public int GetDailyCost(ScientificProjectSpec project)
    {
        return levels.TryGetValue(project.Id, out var l) ? GetDailyCost(project, l) : 0;
    }

    public int GetDailyCost(ScientificProjectSpec project, int level)
    {
        if (project.HasScalingCost)
        {
            var provider = registry.GetCostProviderFor(project.Id);
            return provider.CalculateCost(project, level);
        }

        return project.ScienceCost * level;
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(ScientificProjectsUtils.SaveKey, out var s)) { return; }

        LoadSavedDict(s, ProjectLevelsKey, levels);
        FixSavedDataIfNeeded(levels, "Tomorrow levels");

        LoadSavedDict(s, TodayProjectLevelsKey, todayLevels);
        FixSavedDataIfNeeded(todayLevels, "Today levels");

        if (s.Has(YesterdayScienceKey)) { yesterdayScience = s.Get(YesterdayScienceKey); }
        if (s.Has(ScienceGainedKey)) { ScienceGainedToday = s.Get(ScienceGainedKey); }
        if (s.Has(HasYesterdayScienceKey)) { hasYesterdayScience = s.Get(HasYesterdayScienceKey); }
    }

    void FixSavedDataIfNeeded(Dictionary<string, int> projectLevels, string logDictName)
    {
        foreach (var (id, level) in projectLevels.ToArray())
        {
            if (!registry.ProjectsById.TryGetValue(id, out var spec))
            {
                Debug.LogWarning($"Project {id} not found.");
                projectLevels.Remove(id);
                continue;
            }

            if (!spec.HasSteps)
            {
                Debug.LogWarning($"Project {id} has no steps. Possibly due to an updated mod");
                projectLevels.Remove(id);
                continue;
            }

            if (level > spec.MaxSteps)
            {
                Debug.LogWarning($"Project {id} has {nameof(spec.MaxSteps)} = {spec.MaxSteps}. Level {level} from {logDictName} is invalid and will be turned down.");
                projectLevels[id] = spec.MaxSteps;
            }

            if (level > 0 && !unlocksService.IsUnlocked(id))
            {
                Debug.LogWarning($"Project {id} is not unlocked but has level {level} from {logDictName}. Level will be set to 0.");
                projectLevels[id] = 0;
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(ScientificProjectsUtils.SaveKey);

        s.Set(YesterdayScienceKey, yesterdayScience);
        s.Set(ScienceGainedKey, ScienceGainedToday ?? 0);
        s.Set(HasYesterdayScienceKey, hasYesterdayScience);
        SaveDict(s, ProjectLevelsKey, levels);
        SaveDict(s, TodayProjectLevelsKey, todayLevels);
    }

    void SaveDict(IObjectSaver saver, ListKey<string> key, Dictionary<string, int> dict)
    {
        saver.Set(key, [.. dict.Select(q => $"{q.Key};{q.Value}")]);
    }

    void LoadSavedDict(IObjectLoader loader, ListKey<string> key, Dictionary<string, int> dict)
    {
        dict.Clear();

        if (!loader.Has(key)) { return; }

        foreach (var l in loader.Get(key))
        {
            var parts = l.Split(';');
            dict[parts[0]] = int.Parse(parts[1]);
        }
    }
}

