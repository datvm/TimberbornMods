namespace BetterWeatherStation.Services;

[BindSingleton]
public class WeatherStationInfoService(IEnumerable<IWeatherStationInfoProvider> providers) : ITickableSingleton, ILoadableSingleton
{

    readonly IWeatherStationInfoProvider provider = providers.OrderByDescending(q => q.Order).First();

    public CurrentWeatherStatus CurrentWeatherStatus => provider.Current;

#nullable  disable
    public IReadOnlyList<WeatherDefinition> Weathers { get; private set; }
    public IReadOnlyList<WeatherDefinition> BenignWeathers { get; private set; }
    public IReadOnlyList<WeatherDefinition> HazardousWeathers { get; private set; }
    public FrozenDictionary<string, WeatherDefinition> WeathersById { get; private set; }
    public WeatherDefinition DefaultWeather { get; private set; }   
#nullable enable

    public void Load()
    {
        Weathers = provider.GetWeathers();
        WeathersById = Weathers.ToFrozenDictionary(q => q.Id);
        BenignWeathers = [.. Weathers.Where(q => !q.Hazardous)];
        HazardousWeathers = [.. Weathers.Where(q => q.Hazardous)];

        DefaultWeather = provider.GetDefaultWeather();

        provider.Update();
    }

    public WeatherDefinition GetOrDefault(string id) => WeathersById.TryGetValue(id, out var def) ? def : DefaultWeather;

    public WeatherDefinition GetOrDefault(WeatherStationMode mode) => GetOrDefault(mode switch
    {
        WeatherStationMode.Drought => DefaultWeatherStationInfoProvider.DroughtWeatherId,
        WeatherStationMode.Badtide => DefaultWeatherStationInfoProvider.BadtideWeatherId,
        _ => DefaultWeatherStationInfoProvider.TemperateWeatherId,
    });

    public void Tick()
    {
        provider.Update();
    }

}
