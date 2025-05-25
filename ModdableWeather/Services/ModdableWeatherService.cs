namespace ModdableWeather.Services;

public class ModdableWeatherService(
    ModdableWeatherHistoryProvider historyProvider,
    GameCycleService gameCycleService,
    EventBus eventBus,
    TemperateWeatherDurationService temperateWeatherDurationService,
    HazardousWeatherService hazardousWeatherService
) : WeatherService(eventBus, temperateWeatherDurationService, gameCycleService, hazardousWeatherService), 
    ILoadableSingleton, IUnloadableSingleton
{
    static ModdableWeatherService? instance;
    public static ModdableWeatherService Instance => instance.InstanceOrThrow();

    readonly GameCycleService gameCycleService = gameCycleService;

    public int Cycle => gameCycleService.Cycle;
    public int CycleDay => gameCycleService.CycleDay;
    public float PartialCycleDay => gameCycleService.PartialCycleDay;

    public ModdableWeatherCycle WeatherCycle => historyProvider.CurrentCycle;

    public new int HazardousWeatherDuration => WeatherCycle.HazardousWeatherDuration;
    public new int TemperateWeatherDuration => WeatherCycle.TemperateWeatherDuration;
    public new int HazardousWeatherStartCycleDay => WeatherCycle.HazardousWeatherStartCycleDay;
    public new int CycleLengthInDays => WeatherCycle.CycleLengthInDays;

    public new bool IsHazardousWeather => CycleDay >= HazardousWeatherStartCycleDay;

    public int DurationInDays { get; }

    public int GetWeatherCycleCount(string id) => historyProvider.GetWeatherCycleCount(id);

    void ILoadableSingleton.Load()
    {
        instance = this;
    }

    public void Unload()
    {
        instance = null;
    }

}
