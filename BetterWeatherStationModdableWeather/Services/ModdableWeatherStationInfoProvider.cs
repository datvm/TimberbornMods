namespace BetterWeatherStationModdableWeather.Services;

[MultiBind(typeof(IWeatherStationInfoProvider))]
public class ModdableWeatherStationInfoProvider(
    ModdableWeatherRegistry registry,
    WeatherCycleService weatherCycleService
) : IWeatherStationInfoProvider
{
    public int Order { get; } = 10;
    public CurrentWeatherStatus Current { get; private set; }

    WeatherDefinition defaultWeather = null!;
    FrozenDictionary<string, WeatherDefinition> definitionsById = null!;

    public WeatherDefinition GetDefaultWeather() => defaultWeather;
    static WeatherDefinition FromWeather(IModdableWeather w) => new(w.Id, w.Spec.Display.Value, w.IsHazardous);

    public IReadOnlyList<WeatherDefinition> GetWeathers()
    {
        List<WeatherDefinition> weathers = [];

        foreach (var w in registry.Weathers)
        {
            if (!w.Enabled) { continue; }

            WeatherDefinition def = FromWeather(w);
            weathers.Add(def);

            if (defaultWeather is null && !def.Hazardous)
            {
                defaultWeather = def;
            }
        }

        if (weathers.Count == 0)
        {
            weathers.Add(FromWeather(registry.EmptyBenignWeather));
            weathers.Add(FromWeather(registry.EmptyHazardousWeather));
        }

        defaultWeather ??= weathers.First();
        definitionsById = weathers.ToFrozenDictionary(w => w.Id);

        return weathers;
    }

    WeatherDefinition GetOrDefault(string id) => definitionsById.TryGetValue(id, out var def) ? def : defaultWeather;

    public void Update()
    {
        var curr = weatherCycleService.CurrentStage.Weather.Id;
        var next = weatherCycleService.NextStage.Weather.Id;
        var remaining = weatherCycleService.PartialDaysUntilNextStage;

        Current = new(GetOrDefault(curr), GetOrDefault(next), remaining * 24f);
    }

}
