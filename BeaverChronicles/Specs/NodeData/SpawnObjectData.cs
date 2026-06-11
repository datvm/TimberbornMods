namespace BeaverChronicles.Specs.NodeData;

public record SpawnObjectData
{
    public string[] TemplateNames { get; init; } = [];
    public string X { get; init; } = "";
    public string Y { get; init; } = "";
    public string Z { get; init; } = "";
    public Orientation Orientation { get; init; } = Orientation.Cw0;
    public SpawnObjectConflictMode ConflictMode { get; init; } = SpawnObjectConflictMode.Ignore;
    public string? FailedNodeId { get; init; }
}

public enum SpawnObjectConflictMode
{
    Ignore,
    Destructive
}
