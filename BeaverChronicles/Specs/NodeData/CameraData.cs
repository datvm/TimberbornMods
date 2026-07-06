namespace BeaverChronicles.Specs.NodeData;

public record CameraData
{

    public ImmutableArray<string> FocusOnEntityIds { get; init; } = [];
    public SerializableFloats? Position { get; init; }
    public float? VerticalAngle { get; init; }
    public float? HorizontalAngle { get; init; }
    public float? HorizontalAngel { get; init; }
    public float? ZoomLevel { get; init; }
    public CameraShakeData? CameraShake { get; init; }

    [JsonIgnore]
    public Vector3? PositionValue => Position is { } position ? (Vector3)position : null;

    [JsonIgnore]
    public float? HorizontalAngleValue => HorizontalAngle ?? HorizontalAngel;
}

public readonly record struct CameraShakeData(float Duration = 3f, float Strength = 1f);