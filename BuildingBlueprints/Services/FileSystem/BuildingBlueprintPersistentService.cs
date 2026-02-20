namespace BuildingBlueprints.Services.FileSystem;

[BindSingleton]
public class BuildingBlueprintPersistentService(IExplorerOpener explorerOpener)
{

    public static readonly string BlueprintFolder = Path.Combine(UserDataFolder.Folder, "BuildingBlueprints");
    static BuildingBlueprintPersistentService()
    {
        Directory.CreateDirectory(BlueprintFolder);
        MigrateOldFiles();
    }

    static void MigrateOldFiles()
    {
        var files = Directory.GetFiles(BlueprintFolder, "*.json");

        foreach (var path in files)
        {
            var fileName = Path.GetFileName(path);
            if (!fileName.EndsWith(BuildingBlueprintListingService.FilePostfix, StringComparison.OrdinalIgnoreCase))
            {
                var newFileName = Path.GetFileNameWithoutExtension(fileName) + BuildingBlueprintListingService.FilePostfix;
                var newPath = Path.Combine(BlueprintFolder, newFileName);

                if (File.Exists(newPath))
                {
                    File.Delete(path);
                }
                else
                {
                    File.Move(path, newPath);
                }
            }
        }
    }

    public static ValidationResult ValidateNewBlueprintName(string name, out string? path)
    {
        path = null;
        if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return ValidationResult.Invalid;
        }

        path = GetFilePath(name);
        return File.Exists(path) ? ValidationResult.AlreadyExists : ValidationResult.Valid;
    }

    public static string GetFilePath(string name) => Path.Combine(BlueprintFolder, name + BuildingBlueprintListingService.FilePostfix);

    public static void SaveBlueprintToFile(string name, SerializableBuildingBlueprint blueprint)
    {
        var path = GetFilePath(name);
        File.WriteAllText(path, JsonConvert.SerializeObject(blueprint, Formatting.Indented));
    }

    public static IEnumerable<string> GetBlueprintFiles() => Directory.EnumerateFiles(BlueprintFolder, BuildingBlueprintListingService.FileSearchPattern);

    public void ShowFolder() => explorerOpener.OpenDirectory(BlueprintFolder);

    public enum ValidationResult
    {
        Valid,
        Invalid,
        AlreadyExists
    }

}

