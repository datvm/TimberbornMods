namespace TImprove4Mods.Services;

public class ModCompatibilityService
{
    const string DataUrl = "https://raw.githubusercontent.com/datvm/TimberbornMods/refs/heads/t4mods-repo/mods.json";

    public static readonly ModCompatibilityService Instance = new();
    string modFolder = null!;

    FrozenDictionary<string, ModCompatibility> mods = FrozenDictionary<string, ModCompatibility>.Empty;

    private ModCompatibilityService() { }

    public void Init(string folder)
    {
        modFolder = folder;
        LoadData();
    }

    public void LoadData()
    {
        var path = CachedDataPath;
        if (!File.Exists(path)) { return; }

        try
        {
            var json = File.ReadAllText(path);
            mods = JsonConvert.DeserializeObject<Dictionary<string, ModCompatibility>>(json) 
                ?.ToFrozenDictionary()
                ?? FrozenDictionary<string, ModCompatibility>.Empty;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load mod compatibility data: ");
            Debug.LogError(e);
            mods = FrozenDictionary<string, ModCompatibility>.Empty;
            File.Delete(path);
        }
    }

    public async Task FetchDataAsync()
    {
        string data;
        try
        {
            using var http = new HttpClient();
            data = await http.GetStringAsync(DataUrl);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to fetch mod compatibility data: ");
            Debug.LogError(ex);
            return;
        }
        
        await File.WriteAllTextAsync(CachedDataPath, data);
        LoadData();
    }

    public ModIssue? CheckForFirstIssue(ModRepository modRepo)
    {
        var enabledMods = modRepo.Mods
            .Where(ModPlayerPrefsHelper.IsModEnabled)
            .ToDictionary(q => q.Manifest.Id);

        foreach (var mod in enabledMods.Values)
        {
            if (!mods.TryGetValue(mod.Manifest.Id, out var data)) { continue; }

            // Check for obsolete
            if (data.Obsolete != null)
            {
                return new(mod, data.Obsolete);
            }

            // Check for incompatibles
            if (data.Incompatibles != null)
            {
                foreach (var (id, incompData) in data.Incompatibles)
                {
                    if (!enabledMods.TryGetValue(id, out var incompMod)) { continue; }

                    return new(mod, new IncompatibleModIssue(incompMod, incompData));
                }
            }
        }

        return null;
    }

    string CachedDataPath => Path.Combine(modFolder, "mods.json");

}
