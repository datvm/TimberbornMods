namespace ConfigurableFaction.Services;

public class PersistentService : ILoadableSingleton
{
    const string Postfix = "-faction.json";

    public static readonly string StoragePath = Path.Combine(UserDataFolder.Folder, nameof(ConfigurableFaction));
    public static string GetFactionPath(string factionId) => Path.Combine(StoragePath, factionId + Postfix);

    public ImmutableArray<string> GetSavedFactionIds() 
        => [.. GetPersistentFiles().Select(q => Path.GetFileName(q)[..^Postfix.Length])];

    public void Save<T>(string factionId, T data)
    {
        Debug.Log("Writing data to " + factionId);

        var path = GetFactionPath(factionId);
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public T? Load<T>(string factionId)
    {
        var path = GetFactionPath(factionId);
        if (!File.Exists(path)) { return default; }
        
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public void Load()
    {
        Directory.CreateDirectory(StoragePath);
    }

    public void Clear()
    {
        var files = GetPersistentFiles();
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete file: {file}");
                Debug.LogException(ex);
            }
        }
    }

    static string[] GetPersistentFiles() => Directory.GetFiles(StoragePath, "*" + Postfix);

}
