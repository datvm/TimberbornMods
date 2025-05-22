namespace ModdableWeather.Services;

public class ModdableWeatherRegistry(
    IEnumerable<IModdedTemperateWeather> temperateWeathers,
    IEnumerable<IModdedHazardousWeather> hazardousWeathers,
    NoneHazardousWeather noneHazardousWeather
)
{

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

}
