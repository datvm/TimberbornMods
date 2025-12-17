namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class RefreshingModifier(ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<RefreshingModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Refreshing";
    public override string Id { get; } = ModifierId;
}

public class RefreshingModifierSettings : ModdableWeatherModifierSettings
{

}
