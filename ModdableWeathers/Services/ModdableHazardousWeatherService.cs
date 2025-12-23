namespace ModdableWeathers.Services;

[ReplaceSingleton]
[BypassMethods([
    nameof(Load),
    nameof(Save),
])]
[ThrowMethods(
    nameof(StartHazardousWeather),
    nameof(EndHazardousWeather),
    ExceptionType = typeof(WeatherDayServiceReplacedException)
)]
public class ModdableHazardousWeatherService(WeatherCycleService weatherCycleService)
    : HazardousWeatherService(null, null, null, null, null, null, null),
    ILoadableSingleton
{

    [ReplaceProperty]
    public IHazardousWeather? MCurrentCycleHazardousWeather
        => weatherCycleService.GetCurrentOrNextHazardousWeatherInThisCycle()?.Weather as IHazardousWeather;

    [ReplaceProperty]
    public int MHazardousWeatherDuration
        => weatherCycleService.GetCurrentOrNextHazardousWeatherInThisCycle()?.Stage.Days ?? 0;

}
