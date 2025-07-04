namespace BeaverJukebox.Services;

public class AudioMuteService : ILoadableSingleton, IUnloadableSingleton
{
    public const string MuteSoundName = "Environment.Silent";

    const string MutedSaveKey = $"{nameof(BeaverJukebox)}.MutedList";

    public static AudioMuteService? Instance { get; private set; }

    readonly HashSet<string> muted = [];

    public bool ShouldMute(string key) => muted.Contains(key);
    public void AddMuted(string key) => muted.Add(key);
    public void RemoveMuted(string key) => muted.Remove(key);
    public void ClearMuted() => muted.Clear();
    public void ToggleMuted(string key) => _ = muted.Contains(key) ? muted.Remove(key) : muted.Add(key);

    public void Load()
    {
        Instance = this;
        LoadSavedData();
    }

    void LoadSavedData()
    {
        var settings = PlayerPrefs.GetString(MutedSaveKey, "");
        if (settings == "") { return; }

        var list = settings.Split('|', StringSplitOptions.RemoveEmptyEntries);
        muted.Clear();
        muted.AddRange(list);
    }

    public void Save()
    {
        PlayerPrefs.SetString(MutedSaveKey, string.Join("|", muted));
    }

    public void Unload()
    {
        Save();
        Instance = null;
    }
}
