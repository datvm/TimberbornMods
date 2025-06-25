namespace Redsurge.Weathers;

public class RedsurgeWeather(
    RedsurgeWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService,
    ISingletonLoader loader
) : DefaultModdedWeather<RedsurgeWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedHazardousWeather, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("RedsurgeWeather");
    static readonly PropertyKey<float> PowerModifierKey = new("PowerModifier");

    public const string WeatherId = "Redsurge";
    public override string Id { get; } = WeatherId;

    public float WaterModifier { get; private set; } = 1f;

    public override int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var additional = Settings.RedsurgeMultiplier.Value;
        var parameters = Settings.Parameters;
        var handicap = ModdableWeatherUtils.CalculateHandicap(
            history.GetWeatherCycleCount(Id),
            parameters.HandicapCycles,
            () => parameters.HandicapPerc);

        WaterModifier = 1f + additional * handicap;

        ModdableWeatherUtils.Log(() => $"Redsurge Weather at cycle {cycle} has PowerModifier: {WaterModifier} (handicap: {handicap:#%}).");

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
            && s.Has(PowerModifierKey))
        {
            WaterModifier = s.Get(PowerModifierKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(PowerModifierKey, WaterModifier);
    }
}

