namespace WeatherScientificProjects.Services;

public class WeatherSPWarningExtender(
    ModdableWeatherApproachingTimer timer
) : DefaultProjectUpgradeListener, ILoadableSingleton
{
    const string ModifierId = "WeatherSPWarningExtender";

    public override FrozenSet<string> UnlockListenerIds { get; } = WeatherProjectsUtils.WeatherWarningExtUnlockIds;
    public override FrozenSet<string> DailyListenerIds { get; } = [WeatherProjectsUtils.WeatherWarningExt3Id];
    
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

        timer.AddModifier(new(ModifierId, 10, curr => curr + delta));
    }
}
