namespace BeaverChronicles.Specs.NodeData;

public record CharacterStatusData
{
    public ImmutableArray<string> EntityIds { get; init; } = [];
    public string? RemoveNeed { get; init; }
    public string? InflictNeed { get; init; }
}
