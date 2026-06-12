namespace BeaverChronicles.Specs.NodeData;

public record FindSpawningSpotData
{
    public string[] TemplateNames { get; init; } = [];
    public string ResultName { get; init; } = null!;
    public SerializableInts? LimitArea { get; init; }
    public int NearBuildingRadius { get; init; }
    public string? FailedNodeId { get; init; }
}
