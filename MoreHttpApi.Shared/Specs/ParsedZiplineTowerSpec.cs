namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.ZiplineSystem.ZiplineTowerSpec")]
public record ParsedZiplineTowerSpec(
    HttpSerializableFloats CableAnchorPoint,
    Int32 MaxConnections,
    Int32 MaxDistance,
    HttpSerializableInts[] UnobstructedCoordinates
) : ParsedComponentSpec;