
namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class BadtideModifier (
    ModdableWaterContaminationModifierService contaminationService,
    ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<BadtideModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Badtide";
    public override string Id { get; } = ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        base.Start(stage, history, onLoad);

        contaminationService.AddModifier(new(nameof(BadtideModifier), 1, 3, 10));
    }

    public override void End()
    {
        base.End();

        contaminationService.RemoveModifier(nameof(BadtideModifier));
    }

}

public class BadtideModifierSettings : ModdableWeatherModifierSettings
{
}