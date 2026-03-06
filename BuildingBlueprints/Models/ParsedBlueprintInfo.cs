namespace BuildingBlueprints.Models;

public record ParsedBlueprintInfo(
    SerializableBuildingBlueprint RawInfo,
    ImmutableArray<ParsedBlueprintBuildingPlacement> Buildings,
    ImmutableArray<KeyValuePair<ParsedBlueprintBuilding, int>> BuildingsCount,
    ImmutableArray<GoodAmount> Costs,
    FrozenSet<string> Tags
)
{
    public string Name => RawInfo.Name;
    public ValueTuple<int, int> Size => RawInfo.Size;

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
    Guid? OriginalId,
    Vector3Int Coordinates,
    Orientation Orientation,
    FlipMode Flip,
    JObject? Settings
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

public class BlueprintWithValidation(ParsedBlueprintInfo Blueprint)
{
    public ParsedBlueprintInfo Blueprint { get; } = Blueprint;
    public HashSet<string> MissingTemplates { get; } = [];
    public HashSet<ParsedBlueprintBuilding> LockedTools { get; } = [];

    public bool HasLockedTools => LockedTools.Count > 0;
    public int ScienceCost { get; set; }

    public bool HasMissingTemplates => MissingTemplates.Count > 0;
    public bool Invalid => MissingTemplates.Count > 0 || LockedTools.Count > 0;

}