namespace ModdableWeather.Weathers;

public class ProgressiveTemperateWeather(ProgressiveTemperateWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<ProgressiveTemperateWeatherSettings>(settings, moddableWeatherSpecService), IModdedTemperateWeather
{
    public const string WeatherId = "ProgressiveTemperate";

    public override string Id { get; } = WeatherId;
}

public class ProgressiveTemperateWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = ProgressiveTemperateWeather.WeatherId;
    public override WeatherParameters DefaultSettings { get; } = new(
        Enabled: false,
        StartCycle: 0,
        Chance: 100,
        MinDay: 3,
        MaxDay: 5,
        HandicapPerc: 450,
        HandicapCycles: 10
    );
}