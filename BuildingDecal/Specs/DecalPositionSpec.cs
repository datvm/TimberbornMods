namespace BuildingDecal.Specs;

public enum SizeEdge
{
    None = -1,
    X = 0,
    Y = 1,
    Z = 2,
}

public record DecalPositionSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string NameLoc { get; init; } = null!;
    [Serialize(nameof(NameLoc))]
    public LocalizedText Name { get; init; } = null!;

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public Vector3 Position { get; init; }
    [Serialize]
    public Vector3 Rotation { get; init; }
    [Serialize]
    public SizeEdge ScaleXTo { get; init; } = SizeEdge.None;
    [Serialize]
    public SizeEdge ScaleYTo { get; init; } = SizeEdge.None;

    [Serialize]
    public bool IsDefault { get; init; }

}
