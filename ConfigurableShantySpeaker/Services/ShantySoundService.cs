namespace ConfigurableShantySpeaker.Services;

public class ShantySoundService(
    IExplorerOpener explorerOpener,
    AudioClipManagementService audioClipManagementService
) : ILoadableSingleton
{

    public const string AssetPath = "ShantySpeaker/Sounds";
    public const string Prefix = "Building.";

    public string SoundsPath { get; } = Path.Combine(UserDataFolder.Folder, AssetPath).Replace('\\', '/');

    public ImmutableArray<string> SoundNames { get; private set; } = [];
    public ImmutableHashSet<string> SoundNamesSet { get; private set; } = [];

    public bool HasSound(string soundName) => SoundNamesSet.Contains(soundName);

    public void Load()
    {
        Directory.CreateDirectory(SoundsPath);
        Refresh();
    }

    public void Refresh()
    {
        Clear();

        var files = Directory.GetFiles(SoundsPath, "*.wav", SearchOption.AllDirectories);
        List<string> soundNames = [];
        foreach (var path in files)
        {
            var clip = audioClipManagementService.AddOrReplace(
                path,
                name: Prefix + Path.GetFileNameWithoutExtension(path));

            soundNames.Add(clip.name);
        }

        SoundNames = [.. soundNames.OrderBy(q => q)];
        SoundNamesSet = SoundNames.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
    }

    void Clear()
    {
        foreach (var name in SoundNames)
        {
            audioClipManagementService.Remove(name);
        }
        SoundNames = [];
        SoundNamesSet = [];
    }

    public void OpenSoundsFolder() => explorerOpener.OpenDirectory(SoundsPath);

}
