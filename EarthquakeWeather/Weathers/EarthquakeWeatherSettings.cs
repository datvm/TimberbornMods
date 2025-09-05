namespace EarthquakeWeather.Weathers;

public class EarthquakeWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs,
    ModSettingsBox modSettingsBox
) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = Earthquake.WeatherId;
    public override string? WeatherDescLocKey { get; } = "LV.EQ.EarthquakeDesc";

    public ModSetting<int> MinStrength { get; } = new(10, ModSettingDescriptor
        .CreateLocalized("LV.EQ.MinStr")
        .SetLocalizedTooltip("LV.EQ.MinStrDesc"));
    public ModSetting<int> MaxStrength { get; } = new(20, ModSettingDescriptor
        .CreateLocalized("LV.EQ.MaxStr")
        .SetLocalizedTooltip("LV.EQ.MaxStrDesc"));

    public RangeIntModSetting SurgeStr { get; } = new(70, 0, 100, ModSettingDescriptor
        .CreateLocalized("LV.EQ.SurgeStr")
        .SetLocalizedTooltip("LV.EQ.SurgeStrDesc"));

    public ModSetting<float> MinFrequency { get; } = new(.5f, ModSettingDescriptor
        .CreateLocalized("LV.EQ.MinFreq")
        .SetLocalizedTooltip("LV.EQ.MinFreqDesc"));
    public ModSetting<float> MaxFrequency { get; } = new(.9f, ModSettingDescriptor
        .CreateLocalized("LV.EQ.MaxFreq")
        .SetLocalizedTooltip("LV.EQ.MaxFreqDesc"));

    public RangeIntModSetting InjuryChance { get; } = new(50, 0, 100, ModSettingDescriptor
        .CreateLocalized("LV.EQ.InjuryChance")
        .SetLocalizedTooltip("LV.EQ.InjuryChanceDesc"));
    public RangeIntModSetting BotDurabilityLoss { get; } = new(50, 0, 100, ModSettingDescriptor
        .CreateLocalized("LV.EQ.BotDurabilityLoss")
        .SetLocalizedTooltip("LV.EQ.BotDurabilityLossDesc"));

    public override void SetToDifficulty(WeatherDifficulty difficulty)
    {
        var diff = difficulty switch
        {
            WeatherDifficulty.Easy => EarthquakeDifficultyParameters.Easy,
            WeatherDifficulty.Hard => EarthquakeDifficultyParameters.Hard,
            _ => EarthquakeDifficultyParameters.Normal,
        };

        MinStrength.SetValue(diff.MinStr);
        MaxStrength.SetValue(diff.MaxStr);
        SurgeStr.SetValue(diff.SurgeStr);
        MinFrequency.SetValue(diff.MinFreq);
        MaxFrequency.SetValue(diff.MaxFreq);
        InjuryChance.SetValue(diff.InjuryChance);
        BotDurabilityLoss.SetValue(diff.BotDurabilityLoss);

        base.SetToDifficulty(difficulty);
    }

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);
        return new(
            Enabled: true,
            StartCycle: 1,
            Chance: Mathf.FloorToInt(v.ChanceForBadtide * 100f),
            MinDay: Math.Max(2, v.BadtideDuration.Min),
            MaxDay: v.BadtideDuration.Max,
            HandicapPerc: Mathf.FloorToInt(v.BadtideDurationHandicapMultiplier * 100f),
            HandicapCycles: v.BadtideDurationHandicapCycles
        );
    }

    protected override object GetExportObject() => new EarthquakeExportParameters(
        new(MinStrength.Value, MaxStrength.Value, SurgeStr.Value, MinFrequency.Value, MaxFrequency.Value, InjuryChance.Value, BotDurabilityLoss.Value),
        Parameters);

    public override void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<EarthquakeExportParameters>(value);
        if (parameters == default) { return; }

        ImportFromParameters(parameters.WeatherParameters);

        var eq = parameters.EarthquakeParameters;
        MinStrength.SetValue(eq.MinStr);
        MaxStrength.SetValue(eq.MaxStr);
        SurgeStr.SetValue(eq.SurgeStr);
        MinFrequency.SetValue(eq.MinFreq);
        MaxFrequency.SetValue(eq.MaxFreq);
        InjuryChance.SetValue(eq.InjuryChance);
        BotDurabilityLoss.SetValue(eq.BotDurabilityLoss);
    }
}

public readonly record struct EarthquakeExportParameters(EarthquakeDifficultyParameters EarthquakeParameters, WeatherParameters WeatherParameters);
public readonly record struct EarthquakeDifficultyParameters(int MinStr, int MaxStr, int SurgeStr, float MinFreq, float MaxFreq, int InjuryChance, int BotDurabilityLoss)
{

    public static readonly EarthquakeDifficultyParameters Easy = new(5, 10, 15, .9f, .9f, 25, 25);
    public static readonly EarthquakeDifficultyParameters Normal = new(10, 20, 70, .5f, .9f, 50, 50);
    public static readonly EarthquakeDifficultyParameters Hard = new(30, 60, 100, .3f, .8f, 100, 100);

}