namespace BuildingBlueprints.Services;

public record SerializableBuildingBlueprint(
    string Name,
    ValueTuple<int, int> Size,
    ImmutableArray<SerializableBuildingPlacement> Buildings
);

public readonly record struct SerializableBuildingPlacement(
    string TemplateName,
    ValueTuple<int, int, int> Coordinates,
    Orientation Orientation,
    bool Flip
);