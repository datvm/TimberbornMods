namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class DroughtModifier(ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<DroughtModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Drought";
    public override string Id { get; } = ModifierId;
}

public class DroughtModifierSettings : ModdableWeatherModifierSettings
{

}
