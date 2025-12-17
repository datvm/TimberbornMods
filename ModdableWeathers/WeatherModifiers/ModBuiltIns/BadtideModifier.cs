namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class BadtideModifier (ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<BadtideModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Badtide";
    public override string Id { get; } = ModifierId;
}

public class BadtideModifierSettings : ModdableWeatherModifierSettings
{
}