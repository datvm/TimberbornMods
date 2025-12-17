namespace ModdableWeathers.Common.Settings;

public abstract class BaseWeatherSettingsService<T>(
    IEnumerable<T> settings,
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
    where T : IBaseWeatherSettings
{
    static readonly PropertyKey<string> SettingsKey = new("Settings");
    protected abstract SingletonKey SaveKey { get; }

    protected FrozenDictionary<Type, T> settingsByType = FrozenDictionary<Type, T>.Empty;

    public bool IsNewData { get; private set; }

    public TSetting GetSettings<TSetting>()
        where TSetting : T
        => (TSetting)settingsByType[typeof(TSetting)];

    public void Load()
    {
        settingsByType = settings.ToFrozenDictionary(q => q.GetType());

        IsNewData = !TryLoadData();
        if (IsNewData)
        {
            InitializeNewData();
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

    protected abstract void InitializeNewData();

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
