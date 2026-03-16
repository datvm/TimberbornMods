namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterBuildings.FillValveSpec")]
public record ParsedFillValveSpec(
    Boolean DefaultTargetHeightEnabled,
    Single DefaultTargetHeightOffset,
    Boolean DefaultAutomationTargetHeightEnabled,
    Single DefaultAutomationTargetHeightOffset,
    Single OverflowLimit,
    HttpSerializableInts OutputCoordinates
) : ParsedComponentSpec;