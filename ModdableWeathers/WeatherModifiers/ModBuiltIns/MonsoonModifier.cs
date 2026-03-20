
namespace ModdableWeathers.WeatherModifiers.ModBuiltIns;

public class MonsoonModifier(
    ModdableWaterStrengthModifierService waterStrengthService,
    ModdableWeatherModifierSpecService specs, ModdableWeatherModifierSettingsService settingsService) : ModdableWeatherModifierBase<MonsoonModifierSettings>(specs, settingsService)
{
    public const string ModifierId = "Monsoon";
    public override string Id { get; } = ModifierId;

    public override void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        base.Start(stage, history, onLoad);

        var handicap = ModdableWeathersUtils.CalculateHandicap(
            () => history.GetWeatherModifierOccurrenceCount(ModifierId),
            Settings.HandicapCycles,
            () => Settings.HandicapPercent);
        var multiplier = Mathf.Max(1, Settings.MonsoonMultiplier / 100f * handicap);

        waterStrengthService.AddModifier(new(ModifierId, multiplier, 1, 10, false));
    }

    public override void End()
    {
        base.End();
        waterStrengthService.RemoveModifier(ModifierId);
    }

}

public class MonsoonModifierSettings : ModdableWeatherModifierSettings
{

    [Description("LV.MW.MonsoonMultiplier")]
    public int MonsoonMultiplier { get; set; }

    [Description("LV.MW.HandicapPerc")]
    public int HandicapPercent { get; set; }

    [Description("LV.MW.HandicapCycles")]
    public int HandicapCycles { get; set; }

    public override void InitializeNewSettings()
    {
        MonsoonMultiplier = 350;
        HandicapCycles = 5;
        HandicapPercent = 35;
    }

}
