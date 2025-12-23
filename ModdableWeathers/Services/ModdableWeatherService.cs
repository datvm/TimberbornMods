namespace ModdableWeathers.Services;

[ReplaceSingleton]
[BypassMethods([
    nameof(Load),
])]
public class ModdableWeatherService(
    WeatherCycleService weatherCycleService,
    ModdableHazardousWeatherService hazardous,
    GameCycleService gameCycleService
) : WeatherService(null, null, gameCycleService, null), ILoadableSingleton
{
    public readonly WeatherCycleService WeatherDayService = weatherCycleService;

    [ReplaceProperty]
    public int MCycleLengthInDays => _gameCycleService._cycleDurationInDays;

    [ReplaceProperty]
    public bool MIsHazardousWeather => WeatherDayService.CurrentStage.Weather.IsHazardous;

    [ReplaceProperty]
    public int MHazardousWeatherDuration => hazardous.MHazardousWeatherDuration;

    [ReplaceProperty]
    public int MHazardousWeatherStartCycleDay => WeatherDayService
        .GetCurrentOrNextHazardousWeatherInThisCycle()
        ?.CalculateStartDay()
        ?? 0;

    public IModdableWeather CurrentWeather => WeatherDayService.CurrentStage.Weather;
    public IModdableWeather TomorrowWeather => WeatherDayService.TomorrowStage.Weather;

}
