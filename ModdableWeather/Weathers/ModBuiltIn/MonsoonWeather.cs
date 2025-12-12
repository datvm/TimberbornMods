using ModdableWeather.Services.Registries;

namespace ModdableWeather.Weathers.ModBuiltIn;

public class MonsoonWeather(
    MonsoonWeatherSettings settings,
    ModdableWeatherSpecRegistry moddableWeatherSpecService,
    ISingletonLoader loader
) : DefaultModdedWeather<MonsoonWeatherSettings>(settings, moddableWeatherSpecService),
    IModdableHazardousWeather, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("MonsoonWeather");
    static readonly PropertyKey<float> WaterModifierKey = new("WaterModifier");

    public const string WeatherId = "Monsoon";
    public override string Id { get; } = WeatherId;

    public float WaterModifier { get; private set; } = 1f;

    public override int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        // Decide the strength
        var additional = Settings.MonsoonMultiplier.Value;

        var parameters = Settings.Parameters;
        var handicap = ModdableWeatherUtils.CalculateHandicap(
            history.GetWeatherCycleCount(Id),
            parameters.HandicapCycles,
            () => parameters.HandicapPerc);

        WaterModifier = 1f + additional * handicap;

        ModdableWeatherUtils.LogVerbose(() => $"Monsoon Weather at cycle {cycle} has WaterModifier: {WaterModifier} (handicap: {handicap:#%}).");

        return base.GetDurationAtCycle(cycle, history);
    }

    public override void End()
    {
        WaterModifier = 1f;
        base.End();
    }

    public override void Load()
    {
        base.Load();

        if (loader.TryGetSingleton(SaveKey, out var s)
            && s.Has(WaterModifierKey))
        {
            WaterModifier = s.Get(WaterModifierKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(WaterModifierKey, WaterModifier);
    }
}

public class MonsoonWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecRegistry specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = MonsoonWeather.WeatherId;
    public override int Order { get; } = 5;
    public override string? WeatherDescLocKey { get; } = "LV.MW.MonsoonDesc";
    public ModSetting<float> MonsoonMultiplier { get; } = new(2.5f, ModSettingDescriptor
        .CreateLocalized("LV.MW.MonsoonMultiplier")
        .SetLocalizedTooltip("LV.MW.MonsoonMultiplierDesc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();
        MonsoonMultiplier.Descriptor.SetEnableCondition(() => EnableWeather.Value);
    }

    protected override DefaultWeatherSettingParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static DefaultWeatherSettingParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);

        return new(
            StartCycle: 3,
            Chance: Mathf.FloorToInt(v.ChanceForBadtide * 100f),
            MinDay: v.DroughtDuration.Min,
            MaxDay: v.DroughtDuration.Max,
            HandicapPerc: Mathf.FloorToInt(v.DroughtDurationHandicapMultiplier * 100f),
            HandicapCycles: v.DroughtDurationHandicapCycles
        );
    }

    protected override object GetExportObject() => new MonsoonExportParameters(MonsoonMultiplier.Value, Parameters);

    public override void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<MonsoonExportParameters>(value);
        if (parameters == default) { return; }

        ImportFromParameters(parameters.WeatherParameters);
        MonsoonMultiplier.SetValue(parameters.Multiplier);
    }

}

public readonly record struct MonsoonExportParameters(float Multiplier, DefaultWeatherSettingParameters WeatherParameters);
