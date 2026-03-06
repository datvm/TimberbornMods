namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Buildings.BuildingSpec")]
public record ParsedBuildingSpec(
    string SelectionSoundName,
    string LoopingSoundName,
    ParsedGoodAmountSpec[] BuildingCost,
    Int32 ScienceCost,
    Boolean PlaceFinished,
    Boolean FinishableWithBeaversOnSite,
    Boolean DrawRangeBoundsOnIt
) : ParsedComponentSpec;