namespace BeaverChronicles.Specs.NodeData;

public record ChanceData
{
    public string Value { get; init; } = "";
    public string? SuccessNodeId { get; init; }
    public string? FailNodeId { get; init; }
}
