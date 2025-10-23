

namespace WeatherScientificProjects.Services;

public class WeatherSPWarningExtender(
    ModdableHazardousWeatherApproachingTimer timer
) : DefaultProjectUpgradeListener, IHazardousWeatherApproachingTimerModifier, ILoadableSingleton
{
    public int Order { get; }
    public int Delta { get; private set; } = 0;
    public override FrozenSet<string> UnlockListenerIds { get; } = WeatherProjectsUtils.WeatherWarningExtUnlockIds;
    public override FrozenSet<string> DailyListenerIds { get; } = [WeatherProjectsUtils.WeatherWarningExt3Id];

    public override void Load()
    {
        base.Load();
        timer.RegisterModifier(this);
    }

    public int Modify(int current, int original) => current + Delta;

    protected override void ProcessActiveProjects(IReadOnlyList<ScientificProjectInfo> activeProjects, ScientificProjectSpec? newUnlock, ActiveProjectsSource source)
    {
        var delta = 0;

        foreach (var project in activeProjects)
        {
            if (project.Spec.HasSteps)
            {
                delta += project.Levels.Today * Mathf.RoundToInt(project.Spec.Parameters[0]);
            }
            else
            {
                delta += Mathf.RoundToInt(project.Spec.Parameters[0]);
            }
        }

        Delta = delta;
        timer.CheckForNotification();
    }
}
