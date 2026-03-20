namespace ModdableWeathers.Services;

public class InitialWeatherSettingsService(WatherSettingsExportService exportService)
{

    static readonly string DefaultFilePath = Path.Combine(UserDataFolder.Folder, "moddable-weathers-default.json");

    public void UseCurrent() => exportService.ExportToPath(DefaultFilePath);

    public void ApplyDefaultSettings()
    {
        if (!File.Exists(DefaultFilePath)) { return; }
        exportService.ImportFromPath(DefaultFilePath);
    }

    public void ClearDefault() => File.Delete(DefaultFilePath);

}
