namespace BuildingDecal.Services;

public class BuildingDecalItemSerializer : IValueSerializer<BuildingDecalItem>
{
    public static readonly BuildingDecalItemSerializer Instance = new();
    static readonly PropertyKey<Vector3> PositionKey = new("Position");
    static readonly PropertyKey<Quaternion> RotationKey = new("Rotation");
    static readonly PropertyKey<Vector3> ScaleKey = new("Scale");
    static readonly PropertyKey<string> DecalNameKey = new("DecalName");
    static readonly PropertyKey<Vector3> ColorKey = new("Color");
    static readonly PropertyKey<bool> FlipXKey = new("FlipX");
    static readonly PropertyKey<bool> FlipYKey = new("FlipY");

    public Obsoletable<BuildingDecalItem> Deserialize(IValueLoader valueLoader)
    {
        var s = valueLoader.AsObject();

        return new BuildingDecalItem()
        {
            DecalName = s.Has(DecalNameKey) ? s.Get(DecalNameKey) : DecalPictureService.ErrorIconName,
            Position = s.Has(PositionKey) ? s.Get(PositionKey) : Vector3.zero,
            Rotation = s.Has(RotationKey) ? s.Get(RotationKey) : Quaternion.identity,
            Scale = s.Has(ScaleKey) ? s.Get(ScaleKey) : Vector3.one,
            Color = (s.Has(ColorKey) ? s.Get(ColorKey) : Vector3.one).ToColor(),
            FlipX = s.Has(FlipXKey) && s.Get(FlipXKey),
            FlipY = s.Has(FlipYKey) && s.Get(FlipYKey),
        };
    }

    public void Serialize(BuildingDecalItem value, IValueSaver valueSaver)
    {
        var s = valueSaver.AsObject();
        s.Set(DecalNameKey, value.DecalName);
        s.Set(PositionKey, value.Position);
        s.Set(RotationKey, value.Rotation);
        s.Set(ScaleKey, value.Scale);
        s.Set(ColorKey, value.Color.ToVector3());
        s.Set(FlipXKey, value.FlipX);
        s.Set(FlipYKey, value.FlipY);
    }
}
