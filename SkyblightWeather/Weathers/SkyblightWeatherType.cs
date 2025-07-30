namespace SkyblightWeather.Weathers;

public class SkyblightWeatherType(
    SkyblightWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService
) : DefaultModdedWeather<SkyblightWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedHazardousWeather
{
    public const string WeatherId = "Skyblight";
    public override string Id { get; } = WeatherId;

}

