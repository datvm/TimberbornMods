namespace DecorativePlants.Services;

public static class PersistentService
{

    public const string BlueprintTemplatePath = @"Blueprints/Generated/";
    public const string BuildingsFolderName = "Buildings";
    public const string TemplateCollectionsFolderName = nameof(TemplateCollectionSpec) + "s";
    public const string FactionFolderName = "Factions";

    public static readonly string TemplatePath = Path.Combine(MStarter.ModFolder, BlueprintTemplatePath);
    public static readonly string BuildingsFolder = Path.Combine(TemplatePath, BuildingsFolderName);
    public static readonly string TemplateCollectionsFolder = Path.Combine(TemplatePath, TemplateCollectionsFolderName);

    // This folder is not in our generated folder to overwrite base game files.
    public static readonly string FactionsFolder = Path.Combine(MStarter.ModFolder, FactionFolderName);

    static PersistentService() => EnsurePaths();

    static void EnsurePaths()
    {
        Directory.CreateDirectory(BuildingsFolder);
        Directory.CreateDirectory(TemplateCollectionsFolder);
        Directory.CreateDirectory(FactionsFolder);
    }

    public static string GetTemplateCollectionPath(string collectionId)
        => Path.Combine(TemplateCollectionsFolder, $"{collectionId}.blueprint.json");

    public static string GetBuildingAssetPath(string templateName)
        => $"{BlueprintTemplatePath}{BuildingsFolderName}/{templateName}.blueprint";

    public static string GetBuildingTemplateFilePath(string templateName)
        => Path.Combine(BuildingsFolder, $"{templateName}.blueprint.json");

    public static bool HasData() => Directory.EnumerateFiles(TemplateCollectionsFolder).Any();

    public static string GetFilePathAndPrepareFolder(string relativePath)
    {
        if (relativePath.EndsWith(".blueprint", StringComparison.OrdinalIgnoreCase))
        {
            relativePath += ".json";
        }

        var path = Path.Combine(MStarter.ModFolder, relativePath);

        var folder = Path.GetDirectoryName(path);
        Directory.CreateDirectory(folder);

        return path;
    }

    public static void Clear()
    {
        Directory.Delete(TemplatePath, true);
        Directory.Delete(FactionsFolder, true);

        EnsurePaths();
    }

}
