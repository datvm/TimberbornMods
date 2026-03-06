namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.MechanicalSystem.TransputSpec")]
public record ParsedTransputSpec(
    HttpSerializableInts Coordinates,
    ParsedDirections3D Directions,
    Boolean ReverseRotation
) : ParsedComponentSpec;