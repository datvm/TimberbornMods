
namespace ModdableWeather.Defaults;

public abstract class DefaultWeatherDifficultySettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs,
    ModSettingsBox modSettingsBox
) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{

    public override WeatherParameters DefaultSettings => GetDifficultyParameters(WeatherDifficulty.Normal);

    public WeatherDifficultyModSetting DifficultyButtons { get; private set; } = null!;

    public virtual WeatherDifficulty SupportedDifficulty { get; } = WeatherDifficulty.All;

    protected abstract WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty);

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();

        DifficultyButtons = new(new(SupportedDifficulty));
        DifficultyButtons.OnDifficultyRequested += (_, d) => SetToDifficulty(d);
    }

    public virtual void SetToDifficulty(WeatherDifficulty difficulty)
    {
        var parameters = GetDifficultyParameters(difficulty);

        StartCycle.SetValue(parameters.StartCycle);
        Chance.SetValue(parameters.Chance);
        MinDay.SetValue(parameters.MinDay);
        MaxDay.SetValue(parameters.MaxDay);
        HandicapPerc.SetValue(parameters.HandicapPerc);
        HandicapCycles.SetValue(parameters.HandicapCycles);

        modSettingsBox.CloseAndOpenAgain(ModId, _modRepository);
    }

}

