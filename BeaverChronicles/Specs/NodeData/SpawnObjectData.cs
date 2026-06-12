namespace BeaverChronicles.Specs.NodeData;

public record SpawnObjectData
{
    public string[] TemplateNames { get; init; } = [];
    public string X { get; init; } = "";
    public string Y { get; init; } = "";
    public string Z { get; init; } = "";
    public string Orientation { get; init; } = nameof(Timberborn.Coordinates.Orientation.Cw0);
    public string Flipped { get; init; } = "";
    public string ConflictMode { get; init; } = nameof(SpawnObjectConflictMode.Ignore);
    public string? FailedNodeId { get; init; }
}

public enum SpawnObjectConflictMode
{
    Ignore,
    Destructive
}
