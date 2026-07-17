namespace BetterWeatherStation.Services;

[BindSingleton]
public class WeatherStationInfoService(
    CompatWeatherService compatWeatherService,
    GameCycleService gameCycleService,
    IDayNightCycle dayNightCycle
) : ITickableSingleton, ILoadableSingleton
{
    ICompatWeatherServiceProvider ServiceProvider => compatWeatherService.Provider;

    FrozenDictionary<string, CompatWeatherType> weatherTypes = null!;

    readonly CurrentWeatherStatus current = new();
    public ICurrentWeatherStatus CurrentWeatherStatus => current;

    public IEnumerable<CompatWeatherType> BenignWeathers => ServiceProvider.WeatherTypes.Where(w => w.IsBenign);
    public IEnumerable<CompatWeatherType> HazardousWeathers => ServiceProvider.WeatherTypes.Where(w => !w.IsBenign);

    public void Load()
    {
        weatherTypes = ServiceProvider.WeatherTypes.ToFrozenDictionary(w => w.Id);
        Tick();
    }

    public CompatWeatherType GetOrDefault(string id)
    {
        if (weatherTypes.TryGetValue(id, out var weather))
        {
            return weather;
        }

        return id == CompatWeatherService.TemperateId
            ? ServiceProvider.WeatherTypes.First() // If temperate is missing, just return the first weather type
            : GetOrDefault(CompatWeatherService.TemperateId);
    }

    public CompatWeatherType GetOrDefault(WeatherStationMode mode) => GetOrDefault(mode switch
    {
        WeatherStationMode.Drought => CompatWeatherService.DroughtId,
        WeatherStationMode.Badtide => CompatWeatherService.BadtideId,
        _ => CompatWeatherService.TemperateId,
    });

    public void Tick()
    {
        var cycleNum = gameCycleService.Cycle;
        var cycleDay = gameCycleService.CycleDay;

        if (current.Cycle != cycleNum)
        {
            current.WeatherCycleInfo = ServiceProvider.GetCurrentCycle();
            current.Cycle = cycleNum;
            current.CycleDay = -1;
        }

        if (current.CycleDay != cycleDay)
        {
            current.CycleDay = cycleDay;
            
            var stages = current.WeatherCycleInfo.Stages;
            var currIndex = current.GetCurrentStageIndex(cycleDay);

            var currStage = current.CurrentStage = stages[currIndex];
            current.NextStage = currIndex < stages.Length - 1
                ? stages[currIndex + 1]
                : ServiceProvider.GetNextCycleStage();
            
            current.NextStageCycleDay = currStage.StartDay + currStage.Length;
        }

        var hoursPassedToday = dayNightCycle.HoursPassedToday;
        current.HoursToNextStage = (current.NextStageCycleDay - current.CycleDay) * 24 - hoursPassedToday;
    }

}

