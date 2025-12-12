namespace ModdableWeather.Services.Registries;

public class ModdableWeatherSpecRegistry(
    ISpecService specService,
    ILoc t
) : ILoadableSingleton
{

    public FrozenDictionary<string, ModdableWeatherSpec> Specs { get; private set; } = FrozenDictionary<string, ModdableWeatherSpec>.Empty;
    readonly Dictionary<string, ModdableWeatherSkySpec> cachedSkySpecByIds = [];

#nullable disable
    public ModdableWeatherSpec TemperateWeatherSpec { get; private set; }
    public ModdableWeatherSpec DroughtWeatherSpec { get; private set; }
    public ModdableWeatherSpec BadtideWeatherSpec { get; private set; }

    public ModdableWeatherSkySpec DefaultSkySpec { get; private set; }
#nullable enable

    public void Load()
    {
        Specs = specService.GetSpecs<ModdableWeatherSpec>()
            .ToFrozenDictionary(q => q.Id);

        foreach (var s in Specs.Values)
        {
            var d = s.Display.Value;
            s.MessageStart = t.T("LV.MW.WeatherStart", d);
            s.MessageEnd = t.T("LV.MW.WeatherEnd", d);
            s.MessageApproaching = t.T("LV.MW.WeatherApproaching", d);
            s.MessageInProgress = t.T("LV.MW.WeatherInProgress", d);

            switch (s.Id)
            {
                case GameTemperateWeather.WeatherId:
                    TemperateWeatherSpec = s;
                    break;
                case GameDroughtWeather.WeatherId:
                    DroughtWeatherSpec = s;
                    break;
                case GameBadtideWeather.WeatherId:
                    BadtideWeatherSpec = s;
                    break;
            }
        }

        DefaultSkySpec = TemperateWeatherSpec.GetSpec<ModdableWeatherSkySpec>()
            ?? throw new InvalidDataException($"{nameof(TemperateWeatherSpec)} does not have a sky spec");
    }

    public ModdableWeatherSkySpec GetSkySpec(string? id)
    {
        if (string.IsNullOrEmpty(id)) { return DefaultSkySpec; }

        if (!cachedSkySpecByIds.TryGetValue(id, out var skySpec))
        {
            skySpec = Specs.TryGetValue(id, out var spec) ? GetSkySpec(spec) : DefaultSkySpec;
            cachedSkySpecByIds[id] = skySpec;
        }

        return skySpec;
    }

    public ModdableWeatherSkySpec GetSkySpec(ModdableWeatherSpec spec)
        => spec.GetSpec<ModdableWeatherSkySpec>() ?? DefaultSkySpec;

    public string DefaultTemperateSound => TemperateWeatherSpec.StartSound!;
    public string DefaultDroughtSound => DroughtWeatherSpec.StartSound!;

}
