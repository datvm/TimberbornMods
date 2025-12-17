
namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class RainModifier(ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService)
    : ModdableWeatherModifierBase<RainModifierSettings>(specs, settingsService)
    , IRainEffect
{
    static readonly Color StaticRainColor = new(0.5f, 0.5f, 1f, 0.4f);

    public const string ModifierId = "Rain";

    public override string Id { get; } = ModifierId;

    public Color RainColor { get; } = StaticRainColor;
    public event RainEffectChangedEventHandler OnRainEffectChanged = null!;

    public override void Start(DetailedWeatherCycle cycle, DetailedWeatherCycleStage stage, bool onLoad)
    {
        base.Start(cycle, stage, onLoad);
        OnRainEffectChanged(true);
    }

    public override void End()
    {
        base.End();
        OnRainEffectChanged(false);
    }

}

public class RainModifierSettings : ModdableWeatherModifierSettings
{

}
