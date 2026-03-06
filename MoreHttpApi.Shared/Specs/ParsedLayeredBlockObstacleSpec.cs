namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockObstacles.LayeredBlockObstacleSpec")]
public record ParsedLayeredBlockObstacleSpec(
    HttpSerializableInts LayerSize,
    HttpSerializableFloats AnchorPosition,
    Int32 BlockCreationOffset
) : ParsedComponentSpec;