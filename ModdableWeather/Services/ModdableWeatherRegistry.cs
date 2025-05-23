namespace ModdableWeather.Services;

public class ModdableWeatherRegistry(
    ModdableWeatherSpecService specs,
    IEnumerable<IModdedTemperateWeather> temperateWeathers,
    IEnumerable<IModdedHazardousWeather> hazardousWeathers,
    NoneHazardousWeather noneHazardousWeather,
    GameTemperateWeather gameTemperateWeather,
    GameDroughtWeather gameDroughtWeather,
    GameBadtideWeather gameBadtideWeather
) : ILoadableSingleton
{

    public GameTemperateWeather GameTemperateWeather { get; } = gameTemperateWeather;
    public GameDroughtWeather GameDroughtWeather { get; } = gameDroughtWeather;
    public GameBadtideWeather GameBadtideWeather { get; } = gameBadtideWeather;
    public NoneHazardousWeather NoneHazardousWeather { get; } = noneHazardousWeather;

    public ImmutableHashSet<IModdedTemperateWeather> TemperateWeathers { get; } = [.. temperateWeathers];
    public ImmutableHashSet<IModdedHazardousWeather> HazardousWeathers { get; } = [.. hazardousWeathers];
    public ImmutableHashSet<IModdedWeather> AllWeathers { get; } = [
        .. temperateWeathers,
        .. hazardousWeathers
    ];

    public FrozenDictionary<string, IModdedWeather> WeatherByIds { get; } = 
        temperateWeathers
        .Concat<IModdedWeather>(hazardousWeathers)
        .ToFrozenDictionary(q => q.Id);

    public IModdedTemperateWeather GetTemperateWeather(string id) => (IModdedTemperateWeather)WeatherByIds[id];
    public IModdedHazardousWeather GetHazardousWeather(string id) => (IModdedHazardousWeather)WeatherByIds[id];

    public void Load()
    {
        foreach (var weather in AllWeathers)
        {
            if (specs.Specs.TryGetValue(weather.Id, out var spec))
            {
                weather.Spec = spec;
            } else
            {
                throw new MissingComponentException($"Weather {weather.Id} does not have a {nameof(ModdedWeatherSpec)}.");
            }
        }
    }

}
