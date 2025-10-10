namespace EarthquakeWeather.Weathers;

public class Earthquake(
    EarthquakeWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService,
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle,
    EarthquakeNotificationService earthquakeNotif
) : DefaultModdedWeather<EarthquakeWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedHazardousWeather, ISaveableSingleton, ITickableSingleton
{
    static readonly SingletonKey SaveKey = new("EarthquakeWeather");
    static readonly PropertyKey<Vector2Int> DamageStrengthKey = new("DamageStrength");
    static readonly PropertyKey<float> NextHitKey = new("NextHit");

    public const string WeatherId = "Earthquake";
    public override string Id { get; } = WeatherId;

    public Vector2Int DamageStrength { get; private set; }
    public float SurgeStrength { get; private set; }
    public float NextHit { get; private set; }

    public event Action? OnEarthquakeHit;

    public override int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var parameters = Settings.Parameters;

        var handicap = ModdableWeatherUtils.CalculateHandicap(
            history.GetWeatherCycleCount(Id),
            parameters.HandicapCycles,
            () => parameters.HandicapPerc);

        DamageStrength = new(
            Mathf.FloorToInt(Settings.MinStrength.Value * handicap),
            Mathf.FloorToInt(Settings.MaxStrength.Value * handicap)
        );
        CalculateSurgeStrength();

        ModdableWeatherUtils.Log(() => $"Earthquake Weather at cycle {cycle} has: Damage = {DamageStrength} (handicap: {handicap:#%}).");

        return base.GetDurationAtCycle(cycle, history);
    }

    public override void Load()
    {
        base.Load();

        CalculateSurgeStrength();
        LoadSavedData();

        if (DamageStrength == default)
        {
            // Only happen while testing/editting in editor
            DamageStrength = new(Settings.MinStrength.Value, Settings.MaxStrength.Value);
        }
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(DamageStrengthKey))
        {
            DamageStrength = s.Get(DamageStrengthKey);
        }

        if (s.Has(NextHitKey))
        {
            NextHit = s.Get(NextHitKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(DamageStrengthKey, DamageStrength);
        s.Set(NextHitKey, NextHit);
    }

    float CalculateSurgeStrength() => SurgeStrength = Settings.SurgeStr.Value / 100f;

    public override void Start(bool onLoad)
    {
        if (!onLoad)
        {
            ScheduleNextHit();
        }

        base.Start(onLoad);
    }

    public void Tick()
    {
        if (!Active || NextHit > dayNightCycle.PartialDayNumber) { return; }

        Hit();
    }

    public void Hit()
    {
        earthquakeNotif.Clear();
        OnEarthquakeHit?.Invoke();
        ScheduleNextHit();
        earthquakeNotif.ShowNotification();
    }

    void ScheduleNextHit()
    {
        var delay = UnityEngine.Random.Range(Settings.MinFrequency.Value, Settings.MaxFrequency.Value);
        NextHit = dayNightCycle.PartialDayNumber + delay;
        ModdableWeatherUtils.Log(() => $"Next earthquake hit scheduled at day {NextHit:0.00} (in {delay:0.00} days)");
    }
}

