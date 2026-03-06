namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.NewGameConfigurationSystem.GameModeSpec")]
public record ParsedGameModeSpec(
    Int32 Order,
    Boolean IsDefault,
    string DisplayNameLocKey,
    string DescriptionLocKey,
    Int32 StartingAdults,
    ParsedMinMaxSpec<Single> AdultAgeProgress,
    Int32 StartingChildren,
    ParsedMinMaxSpec<Single> ChildAgeProgress,
    Single FoodConsumption,
    Single WaterConsumption,
    Int32 StartingFood,
    Int32 StartingWater,
    ParsedMinMaxSpec<Int32> TemperateWeatherDuration,
    ParsedMinMaxSpec<Int32> DroughtDuration,
    Single DroughtDurationHandicapMultiplier,
    Int32 DroughtDurationHandicapCycles,
    Int32 CyclesBeforeRandomizingBadtide,
    Single ChanceForBadtide,
    ParsedMinMaxSpec<Int32> BadtideDuration,
    Single BadtideDurationHandicapMultiplier,
    Int32 BadtideDurationHandicapCycles,
    Single InjuryChance,
    Single DemolishableRecoveryRate
) : ParsedComponentSpec;