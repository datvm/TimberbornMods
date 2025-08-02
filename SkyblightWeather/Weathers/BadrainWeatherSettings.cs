namespace SkyblightWeather.Weathers;

public class BadrainWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs,
    ModSettingsBox modSettingsBox
) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = BadrainWeather.WeatherId;

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);
        return new(
            Enabled: true,
            StartCycle: 10,
            Chance: Mathf.FloorToInt(v.ChanceForBadtide / 4 * 100f),
            MinDay: v.BadtideDuration.Min * 4,
            MaxDay: v.BadtideDuration.Max * 4,
            HandicapPerc: Mathf.FloorToInt(v.BadtideDurationHandicapMultiplier * 100f),
            HandicapCycles: v.BadtideDurationHandicapCycles
        );
    }

}
