namespace PowerLines.Specs;

public record PowerLineSettingsSpec : ComponentSpec
{

    [Serialize]
    public int DefaultMaxConnections { get; init; } = 2;

    [Serialize]
    public int DefaultGeneratorMaxConnections { get; init; } = 4;

    [Serialize]
    public float DefaultMaxConnectionLength { get; init; } = 3f;

    [Serialize]
    public float DefaultMaxGeneratorConnectionLength { get; init; } = 5f;

}
