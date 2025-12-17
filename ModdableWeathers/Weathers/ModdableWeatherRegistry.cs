namespace ModdableWeathers.Weathers;

public class ModdableWeatherRegistry(
    IEnumerable<IModdableWeather> weathers,
    EmptyBenignWeather emptyBenignWeather,
    EmptyHazardousWeather emptyHazardousWeather,
    GameTemperateWeather temperateWeather,
    GameDroughtWeather droughtWeather,
    GameBadtideWeather badtideWeather
) : ILoadableSingleton
{
    public ImmutableArray<IModdableWeather> Weathers { get; private set; } = [];
    public ImmutableArray<IModdableBenignWeather> BenignWeathers { get; private set; } = [];
    public ImmutableArray<IModdableHazardousWeather> HazardousWeathers { get; private set; } = [];

    public FrozenDictionary<string, IModdableWeather> WeathersById { get; private set; } = FrozenDictionary<string, IModdableWeather>.Empty;

    public readonly GameTemperateWeather GameTemperateWeather = temperateWeather;
    public readonly GameDroughtWeather GameDroughtWeather = droughtWeather;
    public readonly GameBadtideWeather GameBadtideWeather = badtideWeather;

    public readonly EmptyBenignWeather EmptyBenignWeather = emptyBenignWeather;
    public readonly EmptyHazardousWeather EmptyHazardousWeather = emptyHazardousWeather;

    public void Load()
    {
        Weathers = [.. weathers.OrderBy(q => q.Spec.Order)];
        BenignWeathers = [.. Weathers.OfType<IModdableBenignWeather>()];
        HazardousWeathers = [.. Weathers.OfType<IModdableHazardousWeather>()];

        WeathersById = Weathers.ToFrozenDictionary(q => q.Id);
    }

    public IModdableWeather GetOrFallback(string id, bool isBenign) 
        => WeathersById.TryGetValue(id, out var weather)
            ? weather
            : (isBenign ? EmptyBenignWeather : EmptyHazardousWeather);

    public IModdableWeather? GetOrDefault(string id) => WeathersById.GetOrDefault(id);

}
