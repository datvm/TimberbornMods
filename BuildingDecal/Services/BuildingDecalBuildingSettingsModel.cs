namespace BuildingDecal.Services;

public record BuildingDecalBuildingSettingsModel(
    ImmutableArray<SerializableDecalItem> Decals
)
{

    public static BuildingDecalBuildingSettingsModel From(BuildingDecalComponent component) => new(
        [.. component.DecalItems.Select(SerializableDecalItem.From)]
    );

}

public record SerializableDecalItem(
    string Name,
    SerializableFloats Position,
    SerializableFloats Rotation,
    SerializableFloats Scale,
    SerializableFloats Color,
    bool FlipX,
    bool FlipY
)
{

    public static SerializableDecalItem From(BuildingDecalItem item) => new(
        item.DecalName,
        item.Position,
        item.Rotation,
        item.Scale,
        item.Color,
        item.FlipX,
        item.FlipY
    );

}