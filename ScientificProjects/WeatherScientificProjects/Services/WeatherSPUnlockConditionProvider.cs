namespace WeatherScientificProjects.Services;

public class WeatherSPUnlockConditionProvider(
    ModdableWeatherService weatherService,
    HazardousWeatherApproachingTimer hazardTimer,
    ILoc t
) : IProjectUnlockConditionProvider
{

    public IEnumerable<string> CanCheckUnlockConditionForIds { get; } = WeatherProjectsUtils.EmergencyDrillIds;

    public string? CheckForUnlockCondition(ScientificProjectSpec p)
    {
        if (WeatherProjectsUtils.EmergencyDrillIds.Contains(p.Id))
        {
            return 
                weatherService.HazardousWeatherDuration > 0 
                && hazardTimer.GetModdableWeatherStage() == GameWeatherStage.Warning
                ? null :
                "LV.SP.BadWeatherConditionErr".T(t);
        }
        else
        {
            throw p.ThrowNotSupportedEx();
        }
    }
}
