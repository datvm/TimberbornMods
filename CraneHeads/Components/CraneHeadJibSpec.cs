namespace CraneHeads.Components;

public record CraneHeadJibSpec : ComponentSpec
{
    [Serialize]
    public int ExtraRange { get; init; }
}
