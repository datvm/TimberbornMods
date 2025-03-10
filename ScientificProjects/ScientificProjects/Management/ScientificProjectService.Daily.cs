namespace ScientificProjects.Management;

partial class ScientificProjectService
{
    readonly Dictionary<string, int> levels = [];
    readonly Dictionary<string, int> todayLevels = [];

    public int GetLevel(string id) => levels.TryGetValue(id, out var l) ? l : 0;
    public int GetTodayLevel(string id) => todayLevels.TryGetValue(id, out var l) ? l : 0;
    public void SetLevel(ScientificProjectSpec project, int level)
    {
        if (level < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "Level cannot be negative");
        }

        if (level > project.MaxSteps)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, $"Project {project.Id} has {nameof(project.MaxSteps)} = {project.MaxSteps}");
        }

        levels[project.Id] = level;
        eb.Post(new OnScientificProjectLevelChangeEvent(project, level));
    }

    [OnEvent]
    public void OnDayStart(DaytimeStartEvent _)
    {
        SkipTodayPayment();

        var totalCost = GetTotalTodayCost();
        var have = sciences.SciencePoints;

        if (totalCost > have)
        {
            eb.PostNow(new OnScientificProjectDailyNotEnoughEvent(totalCost, have));
        }
        else
        {
            PayForToday(totalCost);
        }
    }

    public void PayForToday(int total)
    {
        sciences.SubtractPoints(total);

        todayLevels.Clear();
        foreach (var (id, level) in levels)
        {
            if (level == 0) { continue; }

            todayLevels[id] = level;
        }

        eb.Post(new OnScientificProjectDailyCostChargedEvent(total));
    }

    public void SkipTodayPayment()
    {
        todayLevels.Clear();
        eb.Post(new OnScientificProjectDailyCostChargedEvent(0));
    }

    int GetTotalTodayCost()
    {
        var total = 0;

        foreach (var (id, level) in levels)
        {
            if (level == 0) { continue; }

            var spec = GetProject(id);
            var cost = GetCost(spec);
            total += cost;
        }

        return total;
    }

}
