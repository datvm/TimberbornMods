namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class MonsoonModifier(ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<MonsoonModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Monsoon";
    public override string Id { get; } = ModifierId;

}

public class MonsoonModifierSettings : ModdableWeatherModifierSettings
{

    [Description("LV.MW.MonsoonMultiplier")]
    public int MonsoonMultiplier { get; set; } = 250;

}
