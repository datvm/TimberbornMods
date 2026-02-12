namespace BuildingBlueprints.Services;

public record BlueprintSelectionInfo(
    string Name,
    RectInt Area,
    int BaseZ,
    ImmutableArray<BlockObject> BlockObjects,
    ImmutableArray<GoodAmount> Costs,
    ImmutableArray<KeyValuePair<string, int>> BuildingsCount
)
{

    public int AllBuildingsCount => BuildingsCount.Sum(b => b.Value);
    public bool HasAnyBuilding => BuildingsCount.Length > 0;

    public static BlueprintSelectionInfo CreateFromSelection(string name, IEnumerable<BlockObject> blockObjects, RectInt area, int baseZ)
    {
        Dictionary<string, int> costs = [];
        Dictionary<string, int> counters = [];

        foreach (var bo in blockObjects)
        {
            if (!bo) { continue; }

            var template = bo.GetComponent<TemplateSpec>().TemplateName;
            var curr = counters[template] = counters.GetValueOrDefault(template) + 1;

            if (curr == 1) // First building
            {
                var building = bo.GetComponent<BuildingSpec>();
                foreach (var c in building.BuildingCost)
                {
                    costs[c.Id] = costs.GetValueOrDefault(c.Id) + c.Amount;
                }
            }
        }

        return new(
            name,
            area,
            baseZ,
            [..blockObjects],
            [.. costs.Select(kv => new GoodAmount(kv.Key, kv.Value))],
            [.. counters]
        );
    }

}
