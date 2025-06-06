

namespace ModdableWeather.Weathers;

public class ProgressiveTemperateWeather(ProgressiveTemperateWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<ProgressiveTemperateWeatherSettings>(settings, moddableWeatherSpecService), IModdedTemperateWeather
{
    public const string WeatherId = "ProgressiveTemperate";

    public override string Id { get; } = WeatherId;
}

public class ProgressiveTemperateWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    const int FinalMinDay = 3;
    const int TotalHandicapCycles = 15;

    public override string WeatherId { get; } = ProgressiveTemperateWeather.WeatherId;
    public override int Order { get; } = 6;

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);

        int calculatedHandicapPerc = Mathf.CeilToInt(v.TemperateWeatherDuration.Min * 100f / FinalMinDay);

        return new(
            StartCycle: 0,
            Chance: 100,
            MinDay: FinalMinDay,
            MaxDay: 5,
            HandicapCycles: TotalHandicapCycles,
            HandicapPerc: calculatedHandicapPerc
        );
    }
}