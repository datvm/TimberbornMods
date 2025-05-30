namespace ModdableWeather.Weathers.Rain;

public class RainWeather(
    RainWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService,
    GameUISoundController gameUISoundController
) : DefaultModdedWeather<RainWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedTemperateWeather, ILoadableSingleton, IUnloadableSingleton
{
    public static RainWeather? Instance { get; private set; }

    public const string WeatherId = "Rain";
    public override string Id { get; } = WeatherId;

    public override void Load()
    {
        Instance = this;
        base.Load();

        gameUISoundController.PlaySound2D(Spec.StartSound!);
    }

    public void Unload()
    {
        Instance = null;
    }
}

public class RainWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = RainWeather.WeatherId;

    public override WeatherParameters DefaultSettings { get; } = new(
        StartCycle: 1,
        Chance: 50,
        MinDay: 9,
        MaxDay: 12);
}
