namespace ModdableWeathers.WeatherModifiers.Settings;

public class ModdableWeatherModifierSettingsService(IEnumerable<ModdableWeatherModifierSettings> settings, ISingletonLoader loader) : BaseWeatherSettingsService<ModdableWeatherModifierSettings>(settings, loader)
{
    protected override SingletonKey SaveKey { get; } = new(nameof(ModdableWeatherModifierSettingsService));

    // Initialize by the modifier classes
    protected override void InitializeNewData() { }

}
