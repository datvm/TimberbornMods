namespace ModdableTimberbornAchievements.Services;

public class ModdableAchievementUnlocker(EventBus eb) : ILoadableSingleton
{
    public static readonly string FilePath = Path.Combine(PlayerDataFileService.PlayerDataDirectory, "ModdableAchievements.txt");
    readonly HashSet<string> unlocked = [];

    public void Load()
    {
        if (!File.Exists(FilePath)) { return; }

        var lines = File.ReadAllLines(FilePath);
        foreach (var line in lines)
        {
            unlocked.Add(line.Trim());
        }
    }

    public void Save() => File.WriteAllText(FilePath, string.Join(Environment.NewLine, unlocked));

    public void Unlock(IReadOnlyCollection<string> ids)
    {
        if (ids.Count == 0) { return; }

        List<string> unlockedIds = [];
        foreach (var id in ids)
        {
            if (unlocked.Add(id))
            {
                unlockedIds.Add(id);
            }
        }
        if (unlockedIds.Count == 0) { return; }

        eb.Post(new ModdableAchievementUnlockedEvent([.. unlockedIds]));
        Save();
    }

    public void Clear()
    {
        unlocked.Clear();
        Save();
    }

    public bool IsUnlocked(string id) => unlocked.Contains(id);

}

public readonly record struct ModdableAchievementUnlockedEvent(ImmutableArray<string> AchievementIds);