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

    public HashSet<string> Tags { get; init; } = [];

}

public readonly record struct SerializableBuildingPlacement(
    string TemplateName,
    ValueTuple<int, int, int> Coordinates,
    Orientation Orientation,
    bool Flip,
    JObject? Settings
);

public readonly record struct BuildingBlueprintSourceInfo(
    string FilePath,
    bool IsLocal
)
{
    public Sprite GetIcon(NamedIconProvider namedIconProvider) => GetIcon(IsLocal, namedIconProvider);

    public static Sprite GetIcon(bool isLocal, NamedIconProvider namedIconProvider)
    {
        var iconName = isLocal ? "local-file-icon" : "cloud-file-icon";
        return namedIconProvider.GetOrLoad(iconName, "UI/Images/Core/" + iconName);
    }

}