namespace SkyblightWeather.Weathers;

public class BadrainWeather(
    BadrainWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService,
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle
) : DefaultModdedWeather<BadrainWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedHazardousWeather, IRainEffectWeather, IUnloadableSingleton, ISaveableSingleton, ITickableSingleton
{
    static readonly SingletonKey SaveKey = new("BadrainWeather");
    static readonly PropertyKey<bool> ShouldContaminateKey = new("ShouldContaminate");
    static readonly PropertyKey<float> NextContaminationChangeKey = new("NextContaminationChange");

    static readonly Color BadRainColor = new(1f, 0.5f, 0f, 0.4f);

    public const string WeatherId = "Badrain";
    public override string Id { get; } = WeatherId;
    public Color RainColor { get; } = BadRainColor;

    public static int? ShouldReduceMoisture { get; private set; }
    public static bool ShouldContaminateLand { get; private set; }

    float nextContaminationChange;

    public override void Start(bool onLoad)
    {
        base.Start(onLoad);

        if (onLoad)
        {
            LoadSavedData();
        }

        if (Settings.BadrainLimitMoisture.Value)
        {
            ShouldReduceMoisture = Settings.BadrainMoistureRange.Value;
        }
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        ShouldContaminateLand = s.Has(ShouldContaminateKey) && s.Get(ShouldContaminateKey);
        if (s.Has(NextContaminationChangeKey))
        {
            nextContaminationChange = s.Get(NextContaminationChangeKey);
        }
    }

    public override void End()
    {
        base.End();

        ClearState();
        nextContaminationChange = 0;
    }

    public void Unload()
    {
        ClearState();
    }

    void ClearState()
    {
        ShouldReduceMoisture = null;
        ShouldContaminateLand = false;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (!Active) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(ShouldContaminateKey, ShouldContaminateLand);
        s.Set(NextContaminationChangeKey, nextContaminationChange);
    }

    public void Tick()
    {
        if (!Active || !Settings.BadrainContamination.Value) { return; }

        var time = dayNightCycle.PartialDayNumber;
        if (time < nextContaminationChange) { return; }

        ShouldContaminateLand = !ShouldContaminateLand;
        nextContaminationChange = time +
            (ShouldContaminateLand ? Settings.BadrainContaminationDuration.Value : Settings.BadrainClearDuration.Value);
    }

}

