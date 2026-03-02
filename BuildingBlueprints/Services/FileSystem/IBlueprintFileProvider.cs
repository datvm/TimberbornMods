namespace BuildingBlueprints.Services.FileSystem;

public interface IBlueprintFileProvider
{
    bool IsLocal { get; }
    IEnumerable<string> GetBlueprintFiles();
}

public interface ILocalBlueprintFileProvider : IBlueprintFileProvider
{
    bool IBlueprintFileProvider.IsLocal => true;

    void Rename(string oldName, string newName);
    void Delete(string name);
}