namespace WeatherScientificProjects.Processors;

public class ForecastHazardousWeatherApproachingTimerModifier(
    ModdableHazardousWeatherApproachingTimer timer,
    ScientificProjectService projects,
    EventBus eb
) : IHazardousWeatherApproachingTimerModifier, ILoadableSingleton
{
    public int Order { get; }
    public int Delta { get; private set; } = 0;

    public void Load()
    {
        eb.Register(this);
        timer.RegisterModifier(this);
    }

    public int Modify(int current, int original)
    {
        return current + Delta;
    }

    void UpdateDays()
    {
        var delta = 0;

        var activeProjects = WeatherProjectsUtils.WeatherWarningExtIds
            .Select(projects.GetProject)
            .Where(q => q.Unlocked);

        foreach (var project in activeProjects)
        {
            if (project.Spec.HasSteps)
            {
                delta += project.TodayLevel * Mathf.RoundToInt(project.Spec.Parameters[0]);
            }
            else
            {
                delta += Mathf.RoundToInt(project.Spec.Parameters[0]);
            }
        }

        Delta = delta;
        timer.CheckForNotification();
    }

    [OnEvent]
    public void OnProjectsUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        var id = ev.Project.Id;
        if (WeatherProjectsUtils.WeatherWarningExtIds.Contains(id))
        {
            UpdateDays();
        }
    }

    [OnEvent]
    public void OnProjectsPaid(OnScientificProjectDailyCostChargedEvent _)
    {
        UpdateDays();
    }


}
