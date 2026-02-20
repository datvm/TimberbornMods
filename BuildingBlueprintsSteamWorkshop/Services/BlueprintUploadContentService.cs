namespace BuildingBlueprintsSteamWorkshop.Services;

public static class BlueprintUploadContentService
{

    public static readonly string WorkshopTempFolder = Path.Combine(UserDataFolder.Folder, @"Temp\SteamWorkshop\BuildingBlueprints");
    
    public static void PrepareContent(IEnumerable<string> filePaths)
    {
        CleanUp();

        Directory.CreateDirectory(WorkshopTempFolder);
        foreach (var path in filePaths)
        {
            var name = Path.GetFileName(path);
            var target = Path.Combine(WorkshopTempFolder, name);

            File.Copy(path, target);
        }
    }

    public static void CleanUp()
    {
        if (Directory.Exists(WorkshopTempFolder))
        {
            Directory.Delete(WorkshopTempFolder, true);
        }
    }

}
