namespace BeaverChronicles.Specs.NodeData;

public record DeleteEntityData
{
    public ImmutableArray<string> EntityIds { get; init; } = [];
    public string? BeaverDeathMessageLoc { get; init; }
    public SerializableBoundsInts? TerrainBounds { get; init; }

    [JsonIgnore]
    public BoundsInt? TerrainBoundsValue
    {
        get
        {
            if (field is not null) { return field; }
            return TerrainBounds is null ? null : (field = (BoundsInt)TerrainBounds.Value);
        }
    }
}
