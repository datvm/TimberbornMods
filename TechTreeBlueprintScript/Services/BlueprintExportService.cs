namespace TechTreeBlueprintScript.Services;

[BindSingleton]
public class BlueprintExportService
{
    static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
    };

    public async Task WriteGraphAsync(string rootFolder, BlueprintDependencyGraph graph) => await graph.ScanNodesAsync(async n =>
    {
        var building = n.Building;
        var outPath = Path.Combine(rootFolder, building.Blueprint.Path + ".json");
        
        var outFolder = Path.GetDirectoryName(outPath)!;
        Directory.CreateDirectory(outFolder);

        await using var fs = File.Create(outPath);

        List<string> tags = [];
        if (building.IsGatherer)
        {
            tags.Add("Role:Gatherer");
        }
        if (building.IsManufactory)
        {
            tags.Add("Role:Manufactory");
        }
        if (building.IsPlanter)
        {
            tags.Add("Role:Planter");
        }

        foreach (var g in building.Required.Concat(building.Produces))
        {
            tags.Add("Good:" + g);
        }

        var requirements = n.Parents.Select(p => p.Building.TemplateName).Distinct();

        await JsonSerializer.SerializeAsync(fs, new
        {
            TechTreeItemSpec = new TechTreeItemSpec(
                [.. tags],
                [.. requirements]
            )
        }, WriteOptions);
    });

}
