namespace ModdableWeather.Services;

public class ModdableWeatherService(
    ModdableWeatherHistoryProvider history,
    GameCycleService gameCycleService,
    EventBus eventBus,
    TemperateWeatherDurationService temperateWeatherDurationService,
    ModdableHazardousWeatherService hazardousWeatherService
) : WeatherService(eventBus, temperateWeatherDurationService, gameCycleService, hazardousWeatherService),
    ILoadableSingleton, IUnloadableSingleton
{
    static ModdableWeatherService? instance;
    public static ModdableWeatherService Instance => instance.InstanceOrThrow();

    readonly GameCycleService gameCycleService = gameCycleService;

    public int Cycle => gameCycleService.Cycle;
    public int CycleDay => gameCycleService.CycleDay;
    public float PartialCycleDay => gameCycleService.PartialCycleDay;

    public ModdableWeatherCycle WeatherCycle => history.CurrentCycle;
    public ModdableWeatherCycleDetails WeatherCycleDetails => history.CurrentCycleDetails;
    public IModdedWeather CurrentWeather
    {
        get
        {
            var curr = history.CurrentCycleDetails;
            return IsHazardousWeather ? curr.HazardousWeather : curr.TemperateWeather;
        }
    }

    public IModdedWeather NextDayWeather
    {
        get
        {
            var day = CycleDay + 1;
            if (day > WeatherCycle.CycleLengthInDays)
            {
                return history.NextCycleTemperateWeather;
            }
            else if (day >= WeatherCycle.HazardousWeatherStartCycleDay)
            {
                return WeatherCycleDetails.HazardousWeather;
            }
            else
            {
                return WeatherCycleDetails.TemperateWeather;
            }
        }
    }

    public new int HazardousWeatherDuration => WeatherCycle.HazardousWeatherDuration;
    public new int TemperateWeatherDuration => WeatherCycle.TemperateWeatherDuration;
    public new int HazardousWeatherStartCycleDay => WeatherCycle.HazardousWeatherStartCycleDay;
    public new int CycleLengthInDays => WeatherCycle.CycleLengthInDays;

    public new bool IsHazardousWeather => CycleDay >= HazardousWeatherStartCycleDay;

    public bool NextDayIsTemperateWeather() => !NextDayIsHazardousWeather();

    public new void Load()
    {
        instance = this;
        base.Load();
    }

    public void Unload()
    {
        instance = null;
    }

}
