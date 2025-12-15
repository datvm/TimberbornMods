namespace ModdableWeathers.Weathers.ModBuiltIns;

public class RainModdableWeather(ModdableWeatherSpecService specs, ModdableWeatherSettingsService settingsService)
    : DefaultModdableWeatherWithSettings<RainModdableWeatherSettings>(specs, settingsService),
    IRainEffectWeather
{
    public const string WeatherId = "RainWeather";
    static readonly Color StaticRainColor = new(0.5f, 0.5f, 1f, 0.4f);
    
    public override string Id { get; } = WeatherId;
    public Color RainColor { get; } = StaticRainColor;
}

public class RainModdableWeatherSettings : DefaultModdableWeatherSettings
{

    public override void SetTo(GameModeSpec gameMode)
    {
        Enabled = true;
        StartCycle = 0;
        Chance = 100;
        MinDay = Mathf.FloorToInt(gameMode.TemperateWeatherDuration.Min * .75f);
        MaxDay = Mathf.FloorToInt(gameMode.TemperateWeatherDuration.Max * .75f);
    }

}
