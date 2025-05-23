namespace ModdableWeather.Specs;

public record ModdedWeatherSkySpec : ComponentSpec
{

#nullable disable
    [Serialize]
    public ModdedWeatherSkyStageSpec Sunrise { get; init; }
    [Serialize]
    public ModdedWeatherSkyStageSpec Day { get; init; }
    [Serialize]
    public ModdedWeatherSkyStageSpec Sunset { get; init; }
    [Serialize]
    public ModdedWeatherSkyStageSpec Night { get; init; }
#nullable enable

}

public record ModdedWeatherSkyStageSpec : ComponentSpec
{
    [Serialize]
    public Color FogColor { get; init; }
    [Serialize]
    public float FogDensity { get; init; }
}
