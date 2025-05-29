namespace ModdableWeather.Specs;

public record ModdedWeatherSkySpec : ComponentSpec
{

#nullable disable
    [Serialize]
    public FogSettingsSpec Sunrise { get; init; }
    [Serialize]
    public FogSettingsSpec Day { get; init; }
    [Serialize]
    public FogSettingsSpec Sunset { get; init; }
    [Serialize]
    public FogSettingsSpec Night { get; init; }
#nullable enable

    public FogSettingsSpec this[DayStage dayStage] => dayStage switch
    {
        DayStage.Sunrise => Sunrise,
        DayStage.Day => Day,
        DayStage.Sunset => Sunset,
        DayStage.Night => Night,
        _ => throw new ArgumentOutOfRangeException(nameof(dayStage), dayStage, $"Unknown {nameof(DayStage)}"),
    };

}
