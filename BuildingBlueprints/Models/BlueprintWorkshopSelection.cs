namespace BuildingBlueprints.Models;

public class BlueprintWorkshopSelection
{
    public List<SerializableBuildingBlueprint> Blueprints { get; } = [];
    public string ItemId { get; set; } = "";
    public string? ThumbnailPath { get; set; }

    public IEnumerable<string> BlueprintPaths => Blueprints.Select(bp => bp.Source.FilePath);
}
