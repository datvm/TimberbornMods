namespace ModdableWeathers.Services;

[ReplaceSingleton]
[BypassMethods([
    nameof(SetForCycle),
])]
public class ModdableTemperateWeatherDurationService(
    // WeatherCycleService weatherCycleService, // Can't take this because GamePersistentMode needs it
    IContainer container,

    IRandomNumberGenerator randomNumberGenerator, ISceneLoader sceneLoader, ISingletonLoader singletonLoader, MapEditorMode mapEditorMode)
    : TemperateWeatherDurationService(randomNumberGenerator, sceneLoader, singletonLoader, mapEditorMode)
    , ILoadableSingleton
{
#nullable disable
    WeatherCycleService weatherCycleService;
#nullable enable

    void ILoadableSingleton.Load()
    {
        Load(); // Call base Load

        weatherCycleService = container.GetInstance<WeatherCycleService>();
    }

    [ReplaceProperty]
    public int MTemperateWeatherDuration => weatherCycleService.GetNextBenignWeatherInThisCycle()?.Stage.Days ?? 0;

}
