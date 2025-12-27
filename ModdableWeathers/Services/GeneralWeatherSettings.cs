namespace ModdableWeathers.Services;

public class GeneralWeatherSettings(ISingletonLoader loader) : ISaveableSingleton, ILoadableSingleton, IBaseWeatherSettings
{

    static readonly SingletonKey SaveKey = new(nameof(GeneralWeatherSettings));
    static readonly PropertyKey<string> ValueKey = new("Settings");

    [Description("LV.MW.AlwaysShowHazDur")]
    public bool AlwaysShowHazardousDuration { get; set; }

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(ValueKey))
        {
            var json = s.Get(ValueKey);
            ((IBaseWeatherSettings)this).Deserialize(JObject.Parse(json));
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(ValueKey, ((IBaseWeatherSettings)this).Serialize().ToString());
    }

}
