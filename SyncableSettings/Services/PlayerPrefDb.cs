namespace SyncableSettings.Services;

public class PlayerPrefDb
{
    public static readonly PlayerPrefDb Instance = new();
    public static readonly string DatabasePath = Path.Combine(UserDataFolder.Folder, "PlayerData/SyncableSettings.json");

    readonly Dictionary<string, PlayerPrefEntry> entries = [];
    readonly PlayerPrefTimer saveTimer;

    private PlayerPrefDb()
    {
        saveTimer = new(Save);

        if (File.Exists(DatabasePath))
        {
            try
            {
                var json = File.ReadAllText(DatabasePath);
                entries = JsonConvert.DeserializeObject<Dictionary<string, PlayerPrefEntry>>(json) ?? [];
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to load PlayerPrefs from {DatabasePath}. The file may be corrupt: {ex}");
            }
        }
    }

    public bool TryGet(string key, [NotNullWhen(true)] out PlayerPrefEntry? entry)
        => entries.TryGetValue(key, out entry);

    public void DeleteAll()
    {
        entries.Clear();
        Debug.Log("All PlayerPrefs cleared.");
        saveTimer.MarkDirty();
    }

    public void DeleteKey(string key)
    {
        if (entries.Remove(key))
        {
            Debug.Log("Removed PlayerPrefs " + key);
        }
        else
        {
            Debug.LogWarning("Failed to remove PlayerPrefs " + key + ", it may not exist.");
        }
        saveTimer.MarkDirty();
    }

    public bool TryGetFloat(string key, ref float __result)
        => TryGet(key, out var entry) && entry.Type == PlayerPrefEntryType.Float && float.TryParse(entry.Value, out __result);

    public bool TryGetInt(string key, ref int __result)
        => TryGet(key, out var entry) && entry.Type == PlayerPrefEntryType.Int && int.TryParse(entry.Value, out __result);

    public bool TryGetString(string key, ref string __result)
    {
        if (!TryGet(key, out var entry) || entry.Type != PlayerPrefEntryType.String || entry.Value is null)
        {
            return false;
        }

        __result = entry.Value;
        return true;
    }

    public bool TryHasKey(string key, ref bool __result)
    {
        if (!entries.ContainsKey(key)) { return false; }

        return __result = true;
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
            File.WriteAllText(DatabasePath, json);
            Debug.Log("PlayerPrefs saved successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save PlayerPrefs to {DatabasePath}: {ex}");
        }
    }

    public void SetFloat(string key, float value)
    {
        entries[key] = new(key, value.ToString(), PlayerPrefEntryType.Float);
        saveTimer.MarkDirty();
    }

    public void SetInt(string key, int value)
    {
        entries[key] = new(key, value.ToString(), PlayerPrefEntryType.Int);
        saveTimer.MarkDirty();
    }

    public void SetString(string key, string value)
    {
        entries[key] = new(key, value, PlayerPrefEntryType.String);
        saveTimer.MarkDirty();
    }

}
