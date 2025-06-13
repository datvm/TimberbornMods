namespace ModdableWeather.Defaults;

public class GameDroughtWeather(GameDroughtWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<GameDroughtWeatherSettings>(settings, moddableWeatherSpecService), IModdedHazardousWeather
{
    public const string WeatherId = "DroughtWeather";

    public override string Id { get; } = WeatherId;
}

public class GameDroughtWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = GameDroughtWeather.WeatherId;
    public override string ModId { get; } = nameof(ModdableWeather);
    public override int Order { get; } = 2;

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);

        return new(
            Enabled: true,
            StartCycle: 0,
            Chance: Mathf.FloorToInt(100 - v.ChanceForBadtide * 100f),
            MinDay: v.DroughtDuration.Min,
            MaxDay: v.DroughtDuration.Max,
            HandicapPerc: Mathf.FloorToInt(v.DroughtDurationHandicapMultiplier * 100f),
            HandicapCycles: v.DroughtDurationHandicapCycles
        );
    }
}
