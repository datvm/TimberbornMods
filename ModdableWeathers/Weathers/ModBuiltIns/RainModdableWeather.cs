namespace ModdableWeathers.Weathers.ModBuiltIns;

public class RainModdableWeather(ModdableWeatherSpecService specs, ModdableWeatherSettingsService settingsService)
    : DefaultModdableWeatherWithSettings<RainModdableWeatherSettings>(specs, settingsService),
    IModdableBenignWeather
{
    public const string WeatherId = "Rain";
    
    public override string Id { get; } = WeatherId;
}

public class RainModdableWeatherSettings : DefaultModdableWeatherSettings
{

    public override void SetTo(GameModeSpec gameMode)
    {
        StartCycle = 0;
        Chance = 100;
        MinDay = Mathf.FloorToInt(gameMode.TemperateWeatherDuration.Min * .75f);
        MaxDay = Mathf.FloorToInt(gameMode.TemperateWeatherDuration.Max * .75f);
    }

}
