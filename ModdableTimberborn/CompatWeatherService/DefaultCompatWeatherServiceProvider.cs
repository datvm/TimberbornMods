namespace ModdableTimberborn.CompatWeatherService;

[MultiBind(typeof(ICompatWeatherServiceProvider))]
public class DefaultCompatWeatherServiceProvider(
    WeatherService weatherService,
    GameCycleService gameCycleService,
    HazardousWeatherService hazardousWeatherService,
    HazardousWeatherApproachingTimer approachingTimer
) : ICompatWeatherServiceProvider
{
    public static readonly ImmutableArray<CompatWeatherType> GameWeathers = [
        new(CompatWeatherService.TemperateId, "Weather.Temperate", true),
        new(CompatWeatherService.DroughtId, "Weather.Drought", false),
        new(CompatWeatherService.BadtideId, "Weather.Badtide", false),
    ];

    public int Priority { get; } = -1;
    public int ApproachingNotificationDays => approachingTimer._spec.ApproachingNotificationDays;
    public IReadOnlyList<CompatWeatherType> WeatherTypes => GameWeathers;

    public bool IsHazardous => weatherService.IsHazardousWeather;
    public bool IsNextDayHazardous => weatherService.NextDayIsHazardousWeather();

    public string? CurrentCycleHazardousId => weatherService.HazardousWeatherDuration > 0
        ? hazardousWeatherService.CurrentCycleHazardousWeather.Id
        : null;

    public CompatWeatherCycle GetCurrentCycle()
    {
        var temperate = GetCurrentCycleBenignStage();
        var haz = GetCurrentCycleHazardousStage();

        return new(gameCycleService.Cycle,
            haz is null ? [temperate] : [temperate, haz]);
    }

    public CompatWeatherCycleStage GetCurrentCycleStage() 
        => IsHazardous ? GetCurrentCycleHazardousStage()! : GetCurrentCycleBenignStage();

    public CompatNextWeatherCycleStage GetNextCycleStage() =>
        IsHazardous || !(GetCurrentCycleHazardousStage() is { } haz)
            ? new(gameCycleService.Cycle + 1, 0, 1, null, CompatWeatherService.TemperateId, true)
            : (CompatNextWeatherCycleStage)haz;

    public CompatWeatherCycleStage GetCurrentCycleBenignStage()
        => new(gameCycleService.Cycle, 0, 1, weatherService.TemperateWeatherDuration, CompatWeatherService.TemperateId, true);

    public CompatWeatherCycleStage? GetCurrentCycleHazardousStage()
    {
        var hazLen = weatherService.HazardousWeatherDuration;
        if (hazLen == 0) { return null; }

        return new(gameCycleService.Cycle, 1,
            weatherService.HazardousWeatherStartCycleDay,
            hazLen, 
            CurrentCycleHazardousId!,
            false);
    }

    public CompatWeatherWarning GetWarningStatus()
    {
        if (weatherService.HazardousWeatherDuration <= 0)
        {
            return new(CompatWeatherWarningStage.NoHazardous, null, null);
        }

        var daysToHazardous = approachingTimer.DaysToHazardousWeather;
        if (daysToHazardous <=0) // Already hazardous weather
        {
            return new(CompatWeatherWarningStage.Hazardous, daysToHazardous, CurrentCycleHazardousId);
        }

        var warningPeriod = ApproachingNotificationDays;

        return daysToHazardous > warningPeriod
            ? new(CompatWeatherWarningStage.NoWarning, daysToHazardous, CurrentCycleHazardousId)
            : new(Mathf.CeilToInt(daysToHazardous) == warningPeriod ? CompatWeatherWarningStage.ShowedToday : CompatWeatherWarningStage.Showing, daysToHazardous, CurrentCycleHazardousId);
    }

}
