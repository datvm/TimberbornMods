namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterBuildings.TickableWaterBuildingSpec")]
public record ParsedTickableWaterBuildingSpec(
    HttpSerializableInts WaterCoordinates,
    Single MinWaterHeight,
    Single ChangeRange
) : ParsedComponentSpec;