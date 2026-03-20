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
    public bool HasNewSettingEntry { get; private set; }

    public TSetting GetSettings<TSetting>()
        where TSetting : T
        => (TSetting)settingsByType[typeof(TSetting)];

    public virtual void Load()
    {
        settingsByType = settings.ToFrozenDictionary(q => q.GetType());

        IsNewData = !TryLoadData();

        if (IsNewData)
        {
            foreach (var s in settingsByType.Values)
            {
                s.FirstLoad = true;
            }
            HasNewSettingEntry = true;
        }
        else
        {
            HasNewSettingEntry = settingsByType.Any(kv => kv.Value.FirstLoad);
        }
    }

    bool TryLoadData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return false; }

        var json = s.Get(SettingsKey);
        LoadSerializedSettings(json);
        return true;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(SettingsKey, SerializeSettings().ToString());
    }

    public JObject SerializeSettings()
    {
        JObject values = [];
        foreach (var (t, settings) in settingsByType)
        {
            values[t.FullName] = settings.Serialize();
        }

        return values;
    }

    public void LoadSerializedSettings(string json)
    {
        var obj = JObject.Parse(json);
        LoadSerializedSettings(obj);
    }

    public void LoadSerializedSettings(JObject obj)
    {
        foreach (var (t, settings) in settingsByType)
        {
            if (obj.TryGetValue(t.FullName, out var serialized))
            {
                settings.Deserialize(serialized.Value<JObject>()!);
            }
            else
            {
                settings.FirstLoad = true;
            }
        }
    }

}
