
namespace ModdableWeather.Defaults;

public abstract class DefaultPersistentModdedWeather<TSettings>(
    TSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService,
    ISingletonLoader loader
) : DefaultModdedWeather<TSettings>(settings, moddableWeatherSpecService),
    ISaveableSingleton
    where TSettings : DefaultWeatherSettings
{
    public static readonly PropertyKey<bool> ActiveKey = new("Active");

    protected abstract SingletonKey SingletonKey { get; }

    public override void Load()
    {
        base.Load();
        LoadSavedData();
    }

    protected virtual void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SingletonKey, out var s)) { return; }

        Active = s.Has(ActiveKey) && s.Get(ActiveKey);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SingletonKey);
        SaveData(s);
    }

    protected virtual void SaveData(IObjectSaver s)
    {
        if (Active)
        {
            s.Set(ActiveKey, Active);
        }
    }

}
