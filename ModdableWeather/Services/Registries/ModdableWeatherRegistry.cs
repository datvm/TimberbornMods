namespace ModdableWeather.Services.Registries;

public class ModdableWeatherRegistry(
    IEnumerable<ModdableWeatherBase> allWeathers,
    EmptyHazardousWeather emptyHazardousWeather,
    EmptyBenignWeather emptyBenignWeather,
    GameTemperateWeather gameTemperateWeather,
    GameDroughtWeather gameDroughtWeather,
    GameBadtideWeather gameBadtideWeather,
    ModdableWeatherSpecRegistry specs
) : ILoadableSingleton
{

    public GameTemperateWeather GameTemperateWeather { get; } = gameTemperateWeather;
    public GameDroughtWeather GameDroughtWeather { get; } = gameDroughtWeather;
    public GameBadtideWeather GameBadtideWeather { get; } = gameBadtideWeather;

    public EmptyHazardousWeather EmptyHazardousWeather { get; } = emptyHazardousWeather;
    public EmptyBenignWeather EmptyBenignWeather { get; } = emptyBenignWeather;

    public ImmutableArray<ModdableWeatherBase> AllWeathers { get; private set; } = [];
    public FrozenDictionary<string, ModdableWeatherBase> WeatherByIds { get; private set; } = FrozenDictionary<string, ModdableWeatherBase>.Empty;
    public ImmutableArray<ModdableWeatherBase> BenignWeathers { get; private set; } = [];
    public ImmutableArray<ModdableWeatherBase> HazardousWeathers { get; private set; } = [];

    public bool HasWeather(string id) => WeatherByIds.ContainsKey(id);

    public void Load()
    {
        foreach (var w in allWeathers)
        {
            if (!specs.Specs.TryGetValue(w.Id, out var spec))
            {
                throw new Exception($"No spec found for weather with id '{w.Id}' ({w.GetType().FullName})");
            }

            w.Spec = specs.Specs[w.Id];
        }

        AllWeathers = [.. allWeathers.OrderBy(q => q.Spec.Order)];
        WeatherByIds = AllWeathers.ToFrozenDictionary(q => q.Id);
        BenignWeathers = [.. AllWeathers.Where(q => q.IsBenign)];
        HazardousWeathers = [.. AllWeathers.Where(q => q.IsHazardous)];
    }
}
