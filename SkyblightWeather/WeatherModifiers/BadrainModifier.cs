namespace SkyblightWeather.WeatherModifiers;

public class BadrainModifier(
    ModdableWeatherModifierSpecService specs,
    ModdableWeatherModifierSettingsService settingsService,
    RainEffectPlayer rainEffect,
    LandEffectService landEffectService,
    BadrainCharacterApplier characterApplier
)
    : ModdableWeatherModifierBase<BadrainModifierSettings>(specs, settingsService)
{
    static readonly Color RainColor = new(1f, 0.5f, 0f, 0.4f);

    public const string ModifierId = "Badrain";
    public override string Id => ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        rainEffect.AddModifier(new(nameof(BadrainModifier), 10, RainColor));

        var s = Settings;
        if (s.LimitMoisture)
        {
            landEffectService.SetLimitMoistureRange(s.LimitMoistureRange);
        }

        if (s.LandContamination)
        {
            landEffectService.StartContaminatingLand(s.LandContaminationDuration, s.LandClearDuration);
        }

        characterApplier.StartBadrain(s);


        base.Start(stage, history, onLoad);
    }

    public override void End()
    {
        landEffectService.StopLimitingMoisture();
        landEffectService.StopContaminatingLand();
        characterApplier.EndBadrain();
        rainEffect.RemoveModifier(nameof(BadrainModifier));

        base.End();
    }

}
