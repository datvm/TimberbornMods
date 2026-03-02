namespace BuildingBlueprints.Services.FileSystem;


[BindSingleton]
public class BuildingBlueprintListingService(IEnumerable<IBlueprintFileProvider> providers)
{
    public const string FilePostfix = ".building-blueprint.json";
    public const string FileSearchPattern = "*" + FilePostfix;

    readonly ImmutableArray<IBlueprintFileProvider> providers = [.. providers];

    public static string GetNameFromFilePath(string filePath)
    {
        if (filePath.EndsWith(FilePostfix, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileName(filePath)[..^FilePostfix.Length];
        }
        else
        {
            throw new ArgumentException($"File path '{filePath}' does not end with the expected postfix '{FilePostfix}'.", nameof(filePath));
        }
    }

    public IEnumerable<SerializableBuildingBlueprint> GetBlueprints()
    {
        Dictionary<string, int> nameCounters = [];

        foreach (var p in providers)
        {
            var local = p.IsLocal;

            foreach (var path in p.GetBlueprintFiles())
            {
                if (!path.EndsWith(FilePostfix, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"Skipping file '{path}' as it does not have the expected postfix '{FilePostfix}'.");
                    continue;
                }

                SerializableBuildingBlueprint? bp = null;
                try
                {
                    var content = File.ReadAllText(path);
                    bp = JsonConvert.DeserializeObject<SerializableBuildingBlueprint>(content)
                        ?? throw new InvalidDataException("Deserialized blueprint is null");

                    var name = GetNameFromFilePath(path);
                    if (nameCounters.TryGetValue(name, out var count))
                    {
                        name += $" [{count + 1}]";
                        nameCounters[name] = count;
                    }
                    else
                    {
                        nameCounters[name] = 1;
                    }


                    bp.Name = name;
                    bp.Source = new(path, local);

                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load building blueprint from file '{path}': {ex}");
                }

                if (bp is not null)
                {
                    yield return bp;
                }
            }
        }


    }

}
