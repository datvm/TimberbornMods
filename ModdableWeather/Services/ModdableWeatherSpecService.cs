namespace ModdableWeather.Services;

public class ModdableWeatherSpecService(
    ISpecService specService,
    ILoc t
) : ILoadableSingleton
{

    public FrozenDictionary<string, ModdedWeatherSpec> Specs { get; private set; } = FrozenDictionary<string, ModdedWeatherSpec>.Empty;
    readonly Dictionary<string, ModdedWeatherSkySpec> cachedSkySpecByIds = [];

#nullable disable
    public ModdedWeatherSpec TemperateWeatherSpec { get; private set; }
    public ModdedWeatherSpec DroughtWeatherSpec { get; private set; }
    public ModdedWeatherSpec BadtideWeatherSpec { get; private set; }

    public ModdedWeatherSkySpec DefaultSkySpec { get; private set; }
#nullable enable

    public void Load()
    {
        Specs = specService.GetSpecs<ModdedWeatherSpec>()
            .ToFrozenDictionary(q => q.Id);

        foreach (var s in Specs.Values)
        {
            var d = s.Display = t.T(s.DisplayLoc ?? throw new InvalidDataException($"{nameof(s.DisplayLoc)} is null"));
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

        DefaultSkySpec = GetSkySpec(TemperateWeatherSpec)
            ?? throw new InvalidDataException($"{nameof(TemperateWeatherSpec)} does not have a sky spec");
    }

    public ModdedWeatherSkySpec GetSkySpec(string? id)
    {
        if (string.IsNullOrEmpty(id)) { return DefaultSkySpec; }

        if (!cachedSkySpecByIds.TryGetValue(id, out var skySpec))
        {
            skySpec = Specs.TryGetValue(id, out var spec) ? GetSkySpec(spec) : DefaultSkySpec;
            cachedSkySpecByIds[id] = skySpec;
        }

        return skySpec;
    }

    public ModdedWeatherSkySpec GetSkySpec(ModdedWeatherSpec spec)
        => spec.GetSpec<ModdedWeatherSkySpec>() ?? DefaultSkySpec;

    public string DefaultTemperateSound => TemperateWeatherSpec.StartSound!;
    public string DefaultDroughtSound => DroughtWeatherSpec.StartSound!;

}
