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

    public ModSetting<bool> BadrainLimitMoisture { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.MWSb.BadrainLimitMoisture")
        .SetLocalizedTooltip("LV.MWSb.BadrainLimitMoistureDesc"));

    public RangeIntModSetting BadrainMoistureRange { get; } = new(3, 0, 16, ModSettingDescriptor
        .CreateLocalized("LV.MWSb.BadrainMoistureRange")
        .SetLocalizedTooltip("LV.MWSb.BadrainMoistureRangeDesc"));

    public ModSetting<bool> BadrainContamination { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.MWSb.BadrainContamination")
        .SetLocalizedTooltip("LV.MWSb.BadrainContaminationDesc"));

    public ModSetting<float> BadrainContaminationDuration { get; } = new(.01f, ModSettingDescriptor
        .CreateLocalized("LV.MWSb.BadrainContaminationDuration")
        .SetLocalizedTooltip("LV.MWSb.BadrainContaminationDurationDesc"));

    public ModSetting<float> BadrainClearDuration { get; } = new(.5f, ModSettingDescriptor
        .CreateLocalized("LV.MWSb.BadrainClearDuration")
        .SetLocalizedTooltip("LV.MWSb.BadrainClearDurationDesc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        BadrainMoistureRange.Descriptor.SetEnableCondition(() => BadrainLimitMoisture.Value);
        BadrainContaminationDuration.Descriptor.SetEnableCondition(() => BadrainContamination.Value);
        BadrainClearDuration.Descriptor.SetEnableCondition(() => BadrainContamination.Value);
    }

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

    protected override object GetExportObject() => new BadrainExportParameters(
        BadrainLimitMoisture.Value,
        BadrainMoistureRange.Value,
        BadrainContamination.Value,
        BadrainContaminationDuration.Value,
        BadrainClearDuration.Value,
        Parameters
    );

    public override void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<BadrainExportParameters>(value);
        if (parameters == default) { return; }

        ImportFromParameters(parameters.WeatherParameters);
        BadrainLimitMoisture.SetValue(parameters.BadrainLimitMoisture);
        BadrainMoistureRange.SetValue(parameters.BadrainMoistureRange);
        BadrainContamination.SetValue(parameters.BadrainContamination);
        BadrainContaminationDuration.SetValue(parameters.BadrainContaminationDuration);
        BadrainClearDuration.SetValue(parameters.BadrainClearDuration);
    }
}

public readonly record struct BadrainExportParameters(
    bool BadrainLimitMoisture,
    int BadrainMoistureRange,
    bool BadrainContamination,
    float BadrainContaminationDuration,
    float BadrainClearDuration,
    WeatherParameters WeatherParameters
);


