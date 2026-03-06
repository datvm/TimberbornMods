namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.PathSystem.DrivewayModelSpec")]
public record ParsedDrivewayModelSpec(
    ParsedDriveway Driveway,
    Boolean HasCustomCoordinates,
    HttpSerializableInts CustomCoordinates,
    ParsedDirection2D CustomDirection,
    ParsedDrivewayMode DrivewayMode
) : ParsedComponentSpec;