namespace BuildingBlueprints.Services;

public record SerializableBuildingBlueprint(
    ValueTuple<int, int> Size,
    ImmutableArray<SerializableBuildingPlacement> Buildings
)
{
    [JsonIgnore]
    public string Name { get; set; } = "";
}

public readonly record struct SerializableBuildingPlacement(
    string TemplateName,
    ValueTuple<int, int, int> Coordinates,
    Orientation Orientation,
    bool Flip
);