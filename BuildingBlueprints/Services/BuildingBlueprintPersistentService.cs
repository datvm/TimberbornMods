namespace BuildingBlueprints.Services;

[BindSingleton]
public class BuildingBlueprintPersistentService(IExplorerOpener explorerOpener)
{

    public static readonly string BlueprintFolder = Path.Combine(UserDataFolder.Folder, "BuildingBlueprints");
    static BuildingBlueprintPersistentService() => Directory.CreateDirectory(BlueprintFolder);

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

    public static string GetFilePath(string name) => Path.Combine(BlueprintFolder, name + ".json");

    public static void SaveBlueprintToFile(string name, SerializableBuildingBlueprint blueprint)
    {
        var path = GetFilePath(name);
        File.WriteAllText(path, JsonConvert.SerializeObject(blueprint, Formatting.Indented));
    }

    public static IEnumerable<SerializableBuildingBlueprint> GetBlueprints()
    {
        foreach (var file in Directory.EnumerateFiles(BlueprintFolder, "*.json"))
        {
            SerializableBuildingBlueprint? bp = null;
            try
            {
                var content = File.ReadAllText(file);
                bp = JsonConvert.DeserializeObject<SerializableBuildingBlueprint>(content)
                    ?? throw new InvalidDataException("Deserialized blueprint is null");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load building blueprint from file '{file}': {ex}");
            }

            if (bp is not null)
            {
                yield return bp;
            }
        }
    }

    public void ShowFolder() => explorerOpener.OpenDirectory(BlueprintFolder);

    public enum ValidationResult
    {
        Valid,
        Invalid,
        AlreadyExists
    }

}

