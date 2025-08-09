namespace SkyblightWeather.Weathers;

public class SkyblightWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs,
    ModSettingsBox modSettingsBox
) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = SkyblightWeatherType.WeatherId;
    public override string? WeatherDescLocKey { get; } = "LV.MWSb.SkyblightDesc";

    public ModSetting<float> SkyblightStrength { get; } = new(.1f, ModSettingDescriptor
        .CreateLocalized("LV.MWSb.SkyblightStrength")
        .SetLocalizedTooltip("LV.MWSb.SkyblightStrengthDesc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();
        SkyblightStrength.Descriptor.SetEnableCondition(() => EnableWeather.Value);
    }

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);
        return new(
            Enabled: true,
            StartCycle: 7,
            Chance: Mathf.FloorToInt(v.ChanceForBadtide / 2 * 100f),
            MinDay: v.BadtideDuration.Min,
            MaxDay: v.BadtideDuration.Max,
            HandicapPerc: Mathf.FloorToInt(v.BadtideDurationHandicapMultiplier * 100f),
            HandicapCycles: v.BadtideDurationHandicapCycles
        );
    }

    protected override object GetExportObject() => new SkyblightExportParameters(SkyblightStrength.Value, Parameters);

    public override void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<SkyblightExportParameters>(value);
        if (parameters == default) { return; }

        ImportFromParameters(parameters.WeatherParameters);
        SkyblightStrength.SetValue(parameters.Strength);
    }
}

public readonly record struct SkyblightExportParameters(float Strength, WeatherParameters WeatherParameters);

