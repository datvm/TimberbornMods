namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class DroughtModifier(
    ModdableWaterStrengthModifierService waterStrengthService,
    ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService
) : ModdableWeatherModifierBase<DroughtModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Drought";
    public override string Id { get; } = ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        base.Start(stage, history, onLoad);
        waterStrengthService.AddModifier(new(nameof(DroughtModifier), 0f, 3f, 1000));
    }

    public override void End()
    {
        base.End();

        waterStrengthService.RemoveModifier(nameof(DroughtModifier));
    }

}

public class DroughtModifierSettings : ModdableWeatherModifierSettings
{

}
