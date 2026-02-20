namespace BuildingBlueprints.Services.FileSystem;

[MultiBind(typeof(IBlueprintFileProvider))]
public class DocumentFileProvider : IBlueprintFileProvider
{
    public bool IsLocal => true;
    public IEnumerable<string> GetBlueprintFiles() => BuildingBlueprintPersistentService.GetBlueprintFiles();
}
