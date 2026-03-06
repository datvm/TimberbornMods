namespace MoreHttpApi.Services;

public static class PersistenceService
{

    public const string ExportFolder = @$"{nameof(MoreHttpApi)}/Export";
    public static readonly string ExportFolderPath = Path.Combine(UserDataFolder.Folder, ExportFolder);

    public static string GetExportFolder(string? subfolder = null)
    {
        var path = ExportFolderPath;
        if (!string.IsNullOrEmpty(subfolder))
        {
            path = Path.Combine(path, subfolder);
        }
        Directory.CreateDirectory(path);

        return path;
    }

}
