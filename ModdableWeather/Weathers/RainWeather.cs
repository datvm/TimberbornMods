namespace ModdableWeather.Weathers;

public class RainWeather(
    RainWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService
) : DefaultModdedWeather<RainWeatherSettings>(settings, moddableWeatherSpecService),
    IModdedTemperateWeather, ILoadableSingleton, IUnloadableSingleton
{
    public static RainWeather? Instance { get; private set; }
    public static bool IsRaining => Instance?.Active == true;

    public const string WeatherId = "Rain";
    public override string Id { get; } = WeatherId;

    public override void Load()
    {
        Instance = this;
        base.Load();
    }

    public void Unload()
    {
        Instance = null;
    }

}

public class RainWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = RainWeather.WeatherId;
    public override int Order { get; } = 4;

    public ModSetting<bool> EnableRainEffect { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.MW.RainEffect")
        .SetLocalizedTooltip("LV.MW.RainEffectDesc"));

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);

        return new(
            StartCycle: 0,
            Chance: 100,
            MinDay: Mathf.FloorToInt(v.TemperateWeatherDuration.Min * .75f),
            MaxDay: Mathf.FloorToInt(v.TemperateWeatherDuration.Max * .75f)
        );
    }
}
