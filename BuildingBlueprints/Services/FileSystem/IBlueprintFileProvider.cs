namespace BuildingBlueprints.Services.FileSystem;

public interface IBlueprintFileProvider
{
    bool IsLocal { get; }
    IEnumerable<string> GetBlueprintFiles();
}
