namespace WeatherScientificProjects.Services;

public class EmergencyDrillService(
    ModdableWeatherService moddableWeatherService
) : IScientificProjectUpgradeListener
{
    public FrozenSet<string> UnlockListenerIds { get; } = WeatherProjectsUtils.EmergencyDrillIds;
    public FrozenSet<string> ListenerIds { get; } = []; // Not needed

    public void OnDailyPaymentResolved(IReadOnlyList<ScientificProjectInfo> activeProjects) { }

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects) { }

    public void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ExtendTemperateWeather(project);
    }

    void ExtendTemperateWeather(ScientificProjectSpec project)
    {
        var days = Mathf.RoundToInt(project.Parameters[0]);
        moddableWeatherService.ExtendTemperateWeather(days);
    }

}
