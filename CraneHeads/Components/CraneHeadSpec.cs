namespace CraneHeads.Components;

public record CraneHeadSpec : ComponentSpec
{
    [Serialize]
    public Vector3Int AttachmentCoordinates { get; init; }
}
