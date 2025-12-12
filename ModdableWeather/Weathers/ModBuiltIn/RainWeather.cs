using ModdableWeather.Services.Registries;

namespace ModdableWeather.Weathers.ModBuiltIn;

public class RainWeather(
    RainWeatherSettings settings,
    ModdableWeatherSpecRegistry moddableWeatherSpecService
) : DefaultModdedWeather<RainWeatherSettings>(settings, moddableWeatherSpecService),
    IModdableBenignWeather, ILoadableSingleton, IUnloadableSingleton, IRainEffectWeather
{
    static readonly Color StaticRainColor = new(0.5f, 0.5f, 1f, 0.4f);

    public static RainWeather? Instance { get; private set; }
    public static bool IsRaining => Instance?.Active == true;

    public const string WeatherId = "Rain";
    public override string Id { get; } = WeatherId;
    public Color RainColor { get; } = StaticRainColor;

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

public class RainWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecRegistry specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = RainWeather.WeatherId;
    public override int Order { get; } = 4;
    public override string? WeatherDescLocKey { get; } = "LV.MW.RainDesc";

    public ModSetting<bool> EnableRainEffect { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.MW.RainEffect")
        .SetLocalizedTooltip("LV.MW.RainEffectDesc"));

    protected override DefaultWeatherSettingParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static DefaultWeatherSettingParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
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
