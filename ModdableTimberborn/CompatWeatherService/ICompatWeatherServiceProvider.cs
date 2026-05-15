namespace ModdableTimberborn.CompatWeatherService;

public interface ICompatWeatherServiceProvider
{
    IReadOnlyList<CompatWeatherType> WeatherTypes { get; }
    int Priority { get; }
    int ApproachingNotificationDays { get; }
    string? CurrentCycleHazardousId { get; }
    bool IsHazardous { get; }
    bool IsNextDayHazardous { get; }

    CompatWeatherCycle GetCurrentCycle();
    CompatWeatherCycleStage GetCurrentCycleStage();
    CompatWeatherCycleStage? GetCurrentCycleBenignStage();
    CompatWeatherCycleStage? GetCurrentCycleHazardousStage();    
    CompatNextWeatherCycleStage GetNextCycleStage();
    CompatWeatherWarning GetWarningStatus();
}
