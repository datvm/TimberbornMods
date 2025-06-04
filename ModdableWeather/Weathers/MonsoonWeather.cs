namespace ModdableWeather.Weathers;

public class MonsoonWeather(
    MonsoonWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService,
    ISingletonLoader loader
) : DefaultModdedWeather<MonsoonWeatherSettings>(settings, moddableWeatherSpecService), 
    IModdedHazardousWeather,    ISaveableSingleton
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

        ModdableWeatherUtils.Log(() => $"Monsoon Weather at cycle {cycle} has WaterModifier: {WaterModifier} (handicap: {handicap:#%}).");

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

public class MonsoonWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{

    public override string WeatherId { get; } = MonsoonWeather.WeatherId;
    public override WeatherParameters DefaultSettings { get; } = new(
        Enabled: false,
        StartCycle: 3,
        Chance: 100,
        MinDay: 5,
        MaxDay: 9,
        HandicapPerc: 38,
        HandicapCycles: 3
    );

    public ModSetting<float> MonsoonMultiplier { get; } = new(2.5f, ModSettingDescriptor
        .CreateLocalized("LV.MW.MonsoonMultiplier")
        .SetLocalizedTooltip("LV.MW.MonsoonMultiplierDesc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();
        MonsoonMultiplier.Descriptor.SetEnableCondition(() => EnableWeather.Value);
    }

}
