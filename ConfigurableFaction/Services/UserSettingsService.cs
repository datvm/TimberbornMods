namespace ConfigurableFaction.Services;

[BindSingleton(Contexts = BindAttributeContext.Bootstrapper)]
public class UserSettingsService : ILoadableSingleton
{

    const string FileName = "ConfigurableFaction.json";
    public static readonly string FilePath = Path.Combine(UserDataFolder.Folder, FileName);

    public UserSettings Settings { get; private set; } = new();

    public void Load() => Import(FilePath);
    public void Save() => Export(FilePath);

    public void Import(string filePath)
    {
        if (!File.Exists(filePath)) { return; }
        var json = File.ReadAllText(filePath);
        Settings = JsonConvert.DeserializeObject<UserSettings>(json) ?? new();
    }

    public void Export(string filePath)
    {
        var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
        File.WriteAllText(filePath ?? FilePath, json);
    }

}
