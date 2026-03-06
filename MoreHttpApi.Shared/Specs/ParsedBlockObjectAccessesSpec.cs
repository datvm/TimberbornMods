namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockObjectAccesses.BlockObjectAccessesSpec")]
public record ParsedBlockObjectAccessesSpec(
    HttpSerializableInts[] BlockingCoordinates,
    HttpSerializableInts[] AllowedCoordinates
) : ParsedComponentSpec;