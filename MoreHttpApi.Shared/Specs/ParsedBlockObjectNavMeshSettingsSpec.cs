namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockSystemNavigation.BlockObjectNavMeshSettingsSpec")]
public record ParsedBlockObjectNavMeshSettingsSpec(
    Boolean NoAutoWalls,
    Boolean GenerateFloorsOnStackable,
    ParsedBlockObjectNavMeshEdgeGroupSpec[] EdgeGroups,
    ParsedBlockObjectNavMeshUnblockedCoordinatesSpec[] UnblockedCoordinates,
    ParsedBlockObjectNavMeshBlockedEdgeSpec[] BlockedEdges
) : ParsedComponentSpec;