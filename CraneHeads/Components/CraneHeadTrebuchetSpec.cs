namespace CraneHeads.Components;

public record CraneHeadTrebuchetSpec : ComponentSpec
{
    [Serialize]
    public int LaunchDistance { get; init; }
}
