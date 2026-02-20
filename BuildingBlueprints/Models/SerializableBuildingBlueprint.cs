namespace BuildingBlueprints.Models;

public record SerializableBuildingBlueprint(
    ValueTuple<int, int> Size,
    ImmutableArray<SerializableBuildingPlacement> Buildings
)
{

    [JsonIgnore]
    public string Name { get; set; } = "New Blueprint";

    [JsonIgnore]
    public BuildingBlueprintSourceInfo Source { get; set; }
}

public readonly record struct SerializableBuildingPlacement(
    string TemplateName,
    ValueTuple<int, int, int> Coordinates,
    Orientation Orientation,
    bool Flip
);

public readonly record struct BuildingBlueprintSourceInfo(
    string FilePath,
    bool IsLocal
);