namespace ModdableWeathers.WeatherModifiers.Settings;

public class ModdableWeatherModifierSettingsService(IEnumerable<ModdableWeatherModifierSettings> settings, ISingletonLoader loader)
    : BaseWeatherSettingsService<ModdableWeatherModifierSettings>(settings, loader)
{
    protected override SingletonKey SaveKey { get; } = new(nameof(ModdableWeatherModifierSettingsService));

    public override void Load()
    {
        base.Load();
        if (!HasNewSettingEntry) { return; }

        foreach (var s in settingsByType.Values)
        {
            if (!s.FirstLoad) { continue; }

            s.InitializeNewSettings();
        }
    }

}
