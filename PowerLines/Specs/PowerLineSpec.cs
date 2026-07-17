namespace PowerLines.Specs;

public record PowerLineSpec : ComponentSpec
{

    [Serialize]
    public ImmutableArray<Vector3> ConnectionLocations { get; init; } = [];

    [Serialize]
    public int? MaxConnections { get; init; }

    [Serialize]
    public float? MaxConnectionLength { get; init; }

}
