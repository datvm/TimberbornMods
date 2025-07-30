namespace SkyblightWeather.Weathers;

public class BadrainWeather(
    BadrainWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService
) : DefaultModdedWeather<BadrainWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedHazardousWeather, IRainEffectWeather, IUnloadableSingleton
{
    static readonly Color BadRainColor = new(1f, 0.5f, 0f, 0.4f);

    public const string WeatherId = "Badrain";
    public override string Id { get; } = WeatherId;
    public Color RainColor { get; } = BadRainColor;

    public static bool IsRaining { get; private set; }

    public override void Load()
    {
        base.Load();
        IsRaining = Active;
    }

    public override void Start(bool onLoad)
    {
        base.Start(onLoad);
        IsRaining = true;
    }

    public override void End()
    {
        base.End();
        IsRaining = false;
    }

    public void Unload()
    {
        IsRaining = false;
    }
}

