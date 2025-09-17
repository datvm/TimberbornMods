namespace BuildingDecal.Specs;

public record BuildingDecalPositionSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public ImmutableArray<string> PrefabNames { get; init; } = [];

    [Serialize]
    public bool ClearDefaults { get; init; }

    [Serialize]
    public ImmutableArray<string> Positions { get; init; } = [];

}
