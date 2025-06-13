namespace ModdableWeather.Defaults;

public class GameBadtideWeather(
    GameBadtideWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService
) : DefaultModdedWeather<GameBadtideWeatherSettings>(settings, moddableWeatherSpecService), IModdedHazardousWeather
{
    public const string WeatherId = "BadtideWeather";

    public override string Id { get; } = WeatherId;

}

public class GameBadtideWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = GameBadtideWeather.WeatherId;
    public override string ModId { get; } = nameof(ModdableWeather);
    public override int Order { get; } = 3;

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);

        return new(
            Enabled: true,
            StartCycle: v.CyclesBeforeRandomizingBadtide,
            Chance: Mathf.FloorToInt(v.ChanceForBadtide * 100f),
            MinDay: v.BadtideDuration.Min,
            MaxDay: v.BadtideDuration.Max,
            HandicapPerc: Mathf.FloorToInt(v.BadtideDurationHandicapMultiplier * 100f),
            HandicapCycles: v.BadtideDurationHandicapCycles
        );
    }
}
