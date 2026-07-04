namespace ModdableWeathers.Services;

[ReplaceSingleton]
[BypassMethods([
    nameof(Load),
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

    [ReplaceMethod]
    public void MSave(ISingletonSaver singletonSaver)
    {
        // Store the weather key so when player remove the mod, the game can still load the weather key and not crash
        var singleton = singletonSaver.GetSingleton(HazardousWeatherServiceKey);
        singleton.Set(HazardousWeatherDurationKey, HazardousWeatherDuration);
        singleton.Set(IsDroughtKey, 
            CurrentCycleHazardousWeather is null 
            || CurrentCycleHazardousWeather.Id == GameDroughtWeather.WeatherId);
    }

}
