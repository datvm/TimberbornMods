namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockSystemNavigation.BlockObjectNavMeshEdgeGroupSpec")]
public record ParsedBlockObjectNavMeshEdgeGroupSpec(
    Single Cost,
    ParsedBlockObjectNavMeshGroup Group,
    Boolean IsPath,
    ParsedBlockObjectNavMeshEdgeSpec[] AddedEdges
) : ParsedComponentSpec;