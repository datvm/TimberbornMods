namespace SkyblightWeather.WeatherModifiers;

public class SkyblightModifier(
    ModdableWeatherModifierSpecService specs,
    ModdableWeatherModifierSettingsService settingsService,
    BlightApplier blightApplier
)
    : ModdableWeatherModifierBase<SkyblightModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Skyblight";
    public override string Id => ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        blightApplier.Start(Settings.SkyblightStrength / 100f);
        base.Start(stage, history, onLoad);
    }

    public override void End()
    {
        blightApplier.Stop();
        base.End();
    }

}
