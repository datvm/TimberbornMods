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
    readonly ModdableHazardousWeatherService hazardousWeatherService = hazardousWeatherService;

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

    public void ExtendTemperateWeather(int deltaDays)
    {
        if (IsHazardousWeather)
        {
            throw new InvalidOperationException("Hazardous weather already started, cannot extend temperate weather.");
        }

        history.ChangeWeatherDuration(true, TemperateWeatherDuration + deltaDays);
    }

    public void ExtendHazardousWeather(int deltaDays)
    {
        if (history.CurrentHazardousWeather.WeatherId == NoneHazardousWeather.WeatherId)
        {
            return;
        }

        history.ChangeWeatherDuration(false, HazardousWeatherDuration + deltaDays);
    }

    public new void Load()
    {
        instance = this;
        base.Load();

        CurrentWeather.Start(true);
    }

    public void Unload()
    {
        instance = null;
    }

    [OnEvent]
    public void OnCycleWeatherDecided(OnModdableWeatherCycleDecided ev)
    {
        var cycleNo = ev.WeatherCycle.Cycle.Cycle;
        if (ev.WeatherCycle.Cycle.TemperateWeatherDuration > 0)
        {
            ModdableWeatherUtils.Log(() =>
                $"Starting temperate weather {ev.WeatherCycle.TemperateWeather} for cycle {cycleNo}");
            ev.WeatherCycle.TemperateWeather.Start(false);
        }
        else
        {
            ModdableWeatherUtils.Log(() => $"Cycle {cycleNo} does not have a temperate weather.");
        }
    }

    // Don't shadow these or EventBus will cause crashes
    public void NewOnCycleDayStarted()
    {
        if (CycleDay != HazardousWeatherStartCycleDay) { return; }

        if (CycleDay > 1)
        {
            ModdableWeatherUtils.Log(() => $"Ending temperate weather {history.CurrentTemperateWeather} for cycle {Cycle} on day {CycleDay}.");
            history.CurrentTemperateWeather.End();
        }

        ModdableWeatherUtils.Log(() => $"Starting hazardous weather {history.CurrentHazardousWeather} for cycle {Cycle} on day {CycleDay}.");
        history.CurrentHazardousWeather.Start(false);
        hazardousWeatherService.StartHazardousWeather();
    }

    public void NewOnCycleEnded()
    {
        if (HazardousWeatherDuration <= 0)
        {
            ModdableWeatherUtils.Log(() => $"Cycle {Cycle} ended without hazardous weather.");

            var temperateWeather = history.CurrentCycleDetails.TemperateWeather;
            if (temperateWeather.Active)
            {
                ModdableWeatherUtils.Log(() => $"Ending temperate weather {temperateWeather}.");
                temperateWeather.End();
            }

            return;
        }

        ModdableWeatherUtils.Log(() => $"Ending hazardous weather {history.CurrentHazardousWeather} for cycle {Cycle} on day {CycleDay}.");
        history.CurrentHazardousWeather.End();
        hazardousWeatherService.EndHazardousWeather();
    }

}
