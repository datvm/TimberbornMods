namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockSystemNavigation.BlockObjectNavMeshEdgeSpec")]
public record ParsedBlockObjectNavMeshEdgeSpec(
    HttpSerializableInts Start,
    HttpSerializableInts End,
    Boolean IsTwoWay
) : ParsedComponentSpec;