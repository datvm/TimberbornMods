namespace ModdableWeather.Weathers;

public class ShortTemperateWeather(ShortTemperateWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<ShortTemperateWeatherSettings>(settings, moddableWeatherSpecService), IModdedTemperateWeather
{
    public const string WeatherId = "ShortTemperate";

    public override string Id { get; } = WeatherId;
}

public class ShortTemperateWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = ShortTemperateWeather.WeatherId;
    public override WeatherParameters DefaultSettings { get; } = new(
        StartCycle: 10,
        Chance: 20,
        MinDay: 1,
        MaxDay: 2,
        HandicapPerc: 300,
        HandicapCycles: 2
    );
}