
namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class RainModifier(
    RainEffectPlayer rainEffect,
    SoakEffectApplierService soakEffectApplier,
    ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService)
    : ModdableWeatherModifierBase<RainModifierSettings>(specs, settingsService)
{
    static readonly Color RainColor = new(0.5f, 0.5f, 1f, 0.4f);

    public const string ModifierId = "Rain";

    public override string Id { get; } = ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        base.Start(stage, history, onLoad);

        rainEffect.AddModifier(new(nameof(RainModifier), 10, RainColor));
        LandMoistureService.ShouldMoisturize = true;
        LandContaminationBlockerService.ShouldBlock = true;
        soakEffectApplier.StartApplying();
    }

    public override void End()
    {
        base.End();

        rainEffect.RemoveModifier(nameof(RainModifier));
        LandMoistureService.ShouldMoisturize = false;
        LandContaminationBlockerService.ShouldBlock = false;
        soakEffectApplier.StopApplying();
    }

}

public class RainModifierSettings : ModdableWeatherModifierSettings
{

}
