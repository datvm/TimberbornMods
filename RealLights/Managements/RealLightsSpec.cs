namespace RealLights.Managements;

public record RealLightsSpec : ComponentSpec
{

    [Serialize]
    public ImmutableArray<string> PrefabNames { get; init; }

    [Serialize]
    public ImmutableArray<RealLightLightSpec> Lights { get; init; }

}

public record RealLightLightSpec : ComponentSpec
{
    [Serialize(true)]
    public float Range
    {
        get; init => field = value == default ? 3 : value;
    }

    [Serialize]
    public Vector3 Position { get; init; }

    [Serialize(true)]
    public float Intensity
    {
        get; init => field = value == default ? 1 : value;
    }

    [Serialize(true)]
    public Color Color
    {
        get; init => field = value == default ? Color.white : value;
    }

    [Serialize(true)]
    public bool IsNightLight { get; init; }

}
