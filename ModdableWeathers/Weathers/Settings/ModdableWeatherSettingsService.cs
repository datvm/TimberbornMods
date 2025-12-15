namespace ModdableWeathers.Weathers.Settings;

public class ModdableWeatherSettingsService(
    IEnumerable<IModdableWeatherSettings> settings,
    ISingletonLoader loader,
    PersistentGameModeService persistentGameModeService
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ModdableWeatherSettingsService));
    static readonly PropertyKey<string> SettingsKey = new("Settings");

    FrozenDictionary<Type, IModdableWeatherSettings> settingsByType = FrozenDictionary<Type, IModdableWeatherSettings>.Empty;

    public bool IsNewData { get; private set; }

    public TSetting GetSettings<TSetting>()
        where TSetting : IModdableWeatherSettings
        => (TSetting)settingsByType[typeof(TSetting)];

    public void Load()
    {
        settingsByType = settings.ToFrozenDictionary(q => q.GetType());

        IsNewData = !TryLoadData();
        if (IsNewData)
        {
            TrySetWithDifficulty();
        }
    }

    bool TryLoadData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return false; }

        var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(s.Get(SettingsKey)) ?? [];

        foreach (var (t, settings) in settingsByType)
        {
            if (values.TryGetValue(t.FullName, out var serialized))
            {
                settings.Deserialize(serialized);
            }
        }
        return true;
    }

    void TrySetWithDifficulty()
    {
        GameModeSpec[] difficulties = [
            persistentGameModeService.ReconstructedMode,
            persistentGameModeService.BestMatchedMode,
            persistentGameModeService.Default];

        foreach (var s in settingsByType.Values)
        {
            foreach (var d in difficulties)
            {
                if (s.CanSupport(d))
                {
                    s.SetTo(d);
                    break;
                }
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        Dictionary<string, string> values = [];
        foreach (var (t, settings) in settingsByType)
        {
            values[t.FullName] = settings.Serialize();
        }
        s.Set(SettingsKey, JsonConvert.SerializeObject(values));
    }

}
