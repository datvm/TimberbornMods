namespace WeatherScientificProjects.Processors;

public class WarningExtensionProcessor(ScientificProjectService projects, EventBus eb) : ILoadableSingleton, IUnloadableSingleton
{
    static readonly FieldInfo approachingDay = typeof(HazardousWeatherApproachingTimer)
        .GetField(nameof(HazardousWeatherApproachingTimer.ApproachingNotificationDays), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

    static readonly int defaultApproachingDay = (int)approachingDay.GetValue(null);

    public void Load()
    {
        SetApproachingDay();
        eb.Register(this);
    }

    void SetApproachingDay()
    {
        var info = WeatherProjectsUtils.WeatherWarningExtIds
            .Select(projects.GetProject);

        int newValue = defaultApproachingDay;
        foreach (var p in info)
        {
            if (!p.Unlocked) { continue; }
            var mul = p.Spec.HasSteps ? p.Level : 1;

            newValue = (int)MathF.Round(newValue + p.Spec.Parameters[0] * mul);
        }

        approachingDay.SetValue(null, newValue);
    }

    [OnEvent]
    public void OnProjectsUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        if (WeatherProjectsUtils.WeatherWarningExtIds.Contains(ev.Project.Id))
        {
            SetApproachingDay();
        }
    }

    [OnEvent]
    public void OnProjectsPaid(OnScientificProjectDailyCostChargedEvent _)
    {
        SetApproachingDay();
    }

    public void Unload()
    {
        // Reset to default value so player doesn't exploit it
        approachingDay.SetValue(null, defaultApproachingDay);
    }
}
