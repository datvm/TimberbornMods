namespace Redsurge.Weathers;

public class RedsurgeWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs,
    ModSettingsBox modSettingsBox
) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = RedsurgeWeather.WeatherId;
    public ModSetting<float> RedsurgeMultiplier { get; } = new(3.0f, ModSettingDescriptor
        .CreateLocalized("LV.MWRS.RedsurgeMultiplier")
        .SetLocalizedTooltip("LV.MWRS.RedsurgeMultiplierDesc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();
        RedsurgeMultiplier.Descriptor.SetEnableCondition(() => EnableWeather.Value);
    }

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);
        return new(
            Enabled: true,
            StartCycle: 5,
            Chance: Mathf.FloorToInt(v.ChanceForBadtide / 2 * 100f),
            MinDay: v.BadtideDuration.Min,
            MaxDay: v.BadtideDuration.Max,
            HandicapPerc: Mathf.FloorToInt(v.BadtideDurationHandicapMultiplier * 100f),
            HandicapCycles: v.BadtideDurationHandicapCycles
        );
    }

    protected override object GetExportObject() => new RedsurgeExportParameters(RedsurgeMultiplier.Value, Parameters);

    public override void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<RedsurgeExportParameters>(value);
        if (parameters == default) { return; }

        ImportFromParameters(parameters.WeatherParameters);
        RedsurgeMultiplier.SetValue(parameters.Multiplier);
    }
}

public readonly record struct RedsurgeExportParameters(float Multiplier, WeatherParameters WeatherParameters);
