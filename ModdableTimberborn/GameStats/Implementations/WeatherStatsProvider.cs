namespace ModdableTimberborn.GameStats.Implementations;

public class WeatherStatsProvider(CompatWeatherService.CompatWeatherService weatherService) : IStringGameStatProvider
{
    public IEnumerable<string> AvailableStats => [
        GameStats.WeatherCurrent,
        GameStats.WeatherWarningStage,
        GameStats.WeatherWarningNextWeather,
    ];

    public string? GetStat(string statId) => statId switch
    {
        GameStats.WeatherCurrent => weatherService.Provider.GetCurrentCycleStage().WeatherId,
        GameStats.WeatherWarningStage => weatherService.Provider.GetWarningStatus().Stage.ToString(),
        GameStats.WeatherWarningNextWeather => weatherService.Provider.GetWarningStatus().NextWeatherId,
        _ => throw new ArgumentOutOfRangeException(),
    };
}

public class FloatWeatherStatsProvider(CompatWeatherService.CompatWeatherService weatherService) : IFloatGameStatProvider
{
    public IEnumerable<string> AvailableStats => [GameStats.WeatherWarningDaysToHazardous];

    public float GetStat(string statId) => statId switch
    {
        GameStats.WeatherWarningDaysToHazardous => weatherService.Provider.GetWarningStatus().DaysToHazardous ?? -1f,
        _ => throw new ArgumentOutOfRangeException(),
    };
}