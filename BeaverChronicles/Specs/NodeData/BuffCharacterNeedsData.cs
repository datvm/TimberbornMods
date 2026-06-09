namespace BeaverChronicles.Specs.NodeData;

public record BuffCharacterNeedsData
{
    public string? Id { get; init; }
    public ImmutableArray<string> Ids { get; init; } = [];
    public string? Amount { get; init; }
    public bool Permanent { get; init; }
}
