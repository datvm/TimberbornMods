namespace WeatherScientificProjects.Services;

public class WeatherSPUnlockConditionProvider(
    WeatherCycleService weatherCycle,
    ModdableWeatherApproachingTimer weatherTimer,
    ILoc t
) : IProjectUnlockConditionProvider
{

    public IEnumerable<string> CanCheckUnlockConditionForIds { get; } = WeatherProjectsUtils.EmergencyDrillIds;

    public string? CheckForUnlockCondition(ScientificProjectSpec p) =>
        WeatherProjectsUtils.EmergencyDrillIds.Contains(p.Id)
            ? weatherCycle.NextStage.Weather.IsHazardous
                && weatherTimer.GetNextWeatherWarningProgress() >= 0f
                ? null
                : "LV.SP.BadWeatherConditionErr".T(t)
            : throw p.ThrowNotSupportedEx();
}
