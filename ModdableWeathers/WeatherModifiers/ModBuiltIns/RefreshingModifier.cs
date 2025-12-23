namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class RefreshingModifier(
    ModdableWaterContaminationModifierService contaminationService,
    ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<RefreshingModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Refreshing";
    public override string Id { get; } = ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        base.Start(stage, history, onLoad);
        contaminationService.AddModifier(new(nameof(RefreshingModifier), -1, 3, 10));
    }

    public override void End()
    {
        base.End();
        contaminationService.RemoveModifier(nameof(RefreshingModifier));
    }

}

public class RefreshingModifierSettings : ModdableWeatherModifierSettings
{

}
