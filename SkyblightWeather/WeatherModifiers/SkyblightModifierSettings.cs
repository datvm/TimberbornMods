namespace SkyblightWeather.WeatherModifiers;

public class SkyblightModifierSettings : ModdableWeatherModifierSettings
{

    [Description("LV.MWSb.SkyblightStrength")]
    public int SkyblightStrength { get; set; }

    public override void InitializeNewSettings()
    {
        SkyblightStrength = 10;
    }

}
