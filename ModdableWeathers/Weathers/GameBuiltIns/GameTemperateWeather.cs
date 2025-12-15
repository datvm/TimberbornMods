namespace ModdableWeathers.Weathers.GameBuiltIns;

public class GameTemperateWeather(ModdableWeatherSpecService specs, ModdableWeatherSettingsService settingsService)
    : DefaultModdableWeatherWithSettings<GameTemperateWeatherSettings>(specs, settingsService), IModdableBenignWeather
{

    public const string WeatherId = "TemperateWeather";

    public override string Id { get; } = WeatherId;
}

public class GameTemperateWeatherSettings : DefaultModdableWeatherSettings
{
    public override void SetTo(GameModeSpec gameMode)
    {
        Enabled = true;
        StartCycle = 0;
        Chance = 100;
        MinDay = gameMode.TemperateWeatherDuration.Min;
        MaxDay = gameMode.TemperateWeatherDuration.Max;
    }
}
