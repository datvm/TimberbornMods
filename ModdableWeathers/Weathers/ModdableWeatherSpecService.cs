namespace ModdableWeathers.Weathers;

public class ModdableWeatherSpecService(
    ISpecService specs,
    ILoc t
) : ILoadableSingleton, IUnloadableSingleton
{

    static ModdableWeatherSpecService? instance;
    public static ModdableWeatherSpecService Instance => instance.InstanceOrThrow();

    public ImmutableArray<ModdableWeatherSpec> AllSpecs { get; private set; } = [];
    public FrozenDictionary<string, ModdableWeatherSpec> SpecsById { get; private set; } = FrozenDictionary<string, ModdableWeatherSpec>.Empty;

    public FrozenDictionary<string, ModdableWeatherSkySpec> FogSpecsById { get; private set; } = FrozenDictionary<string, ModdableWeatherSkySpec>.Empty;
    public ModdableWeatherSkySpec DefaultFogSettingsSpec { get; private set; } = null!;

    public void Load()
    {
        instance = this;

        AllSpecs = [.. specs.GetSpecs<ModdableWeatherSpec>()];
        SpecsById = AllSpecs.ToFrozenDictionary(spec => spec.Id);

        FogSpecsById = AllSpecs
            .Select(s => (s.Id, s.GetSpec<ModdableWeatherSkySpec>()))
            .Where(s => s.Item2 is not null)
            .ToFrozenDictionary(s => s.Id, s => s.Item2!);
        DefaultFogSettingsSpec = FogSpecsById[GameTemperateWeather.WeatherId];

        foreach (var s in AllSpecs)
        {
            var d = s.Display.Value;
            s.MessageStart = t.T("LV.MW.WeatherStart", d);
            s.MessageEnd = t.T("LV.MW.WeatherEnd", d);
            s.MessageApproaching = t.T("LV.MW.WeatherApproaching", d);
            s.MessageInProgress = t.T("LV.MW.WeatherInProgress", d);
        }
    }

    public ModdableWeatherSkySpec GetFogSpecForWeather(string id) =>
        FogSpecsById.TryGetValue(id, out var spec) ? spec : DefaultFogSettingsSpec;

    public void Unload()
    {
        instance = null;
    }
}
