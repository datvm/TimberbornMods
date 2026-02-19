namespace BuildingBlueprints.Services;

public record ParsedBlueprintInfo(
    string Name,
    Vector2Int Size,
    ImmutableArray<ParsedBlueprintBuildingPlacement> Buildings,
    ImmutableArray<KeyValuePair<ParsedBlueprintBuilding, int>> BuildingsCount,
    ImmutableArray<GoodAmount> Costs
)
{
    public IEnumerable<ValueTuple<string, int>> TemplatesAndCount
        => BuildingsCount.Select(kv => (kv.Key.TemplateName, kv.Value));

    public ParsedBlueprintBuildingPlacement FirstBuilding => Buildings
        .OrderBy(b => b.Coordinates.z)
        .ThenBy(b => b.Coordinates.x)
        .ThenBy(b => b.Coordinates.y)
        .First();


}

public record ParsedBlueprintBuildingPlacement(
    ParsedBlueprintBuilding Building,
    Vector3Int Coordinates,
    Orientation Orientation,
    FlipMode Flip
);

public record ParsedBlueprintBuilding(
    string TemplateName,
    PlaceableBlockObjectSpec? PlaceableSpec,
    LabeledEntitySpec? LabeledEntitySpec,
    BuildingSpec? BuildingSpec,
    BlockObjectSpec? BlockObjectSpec
)
{
    public bool Missing => PlaceableSpec is null || LabeledEntitySpec is null || BuildingSpec is null || BlockObjectSpec is null;
}

public readonly record struct BlueprintWithValidation(ParsedBlueprintInfo Blueprint)
{
    public HashSet<string> MissingTemplates { get; } = [];
    public HashSet<ParsedBlueprintBuilding> LockedTools { get; } = [];

    public bool Invalid => MissingTemplates.Count > 0 || LockedTools.Count > 0;
}