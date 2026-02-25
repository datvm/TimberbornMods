namespace HueAndTurnX.Components;

public abstract record SerializableModelBase
{
    public string Serialize() => JsonConvert.SerializeObject(this);

    public abstract bool IsDefault { get; }
}

public record SerializableColorModel(
    SerializableFloats? Color,
    float? Transparency
) : SerializableModelBase
{
    public static readonly SerializableColorModel Default = new(null, null);

    public override bool IsDefault => this == Default;
}

public record SerializablePositionsModel(
    SerializableFloats? Rotation,
    SerializableFloats? Translation,
    SerializableFloats? Scale
) : SerializableModelBase
{
    public static readonly SerializablePositionsModel Default = new(null, null, null);

    public override bool IsDefault => this == Default;
}