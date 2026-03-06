namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Buildings.BuildingModelSpec")]
public record ParsedBuildingModelSpec(
    string FinishedModelName,
    string UnfinishedModelName,
    string FinishedUncoveredModelName,
    string UndergroundModelName,
    ParsedConstructionModeModel ConstructionModeModel,
    Int32 UndergroundModelDepth
) : ParsedComponentSpec;