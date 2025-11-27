namespace ModdableTimberbornAchievements.Services;

public class ModdableStoreAchievement(
    IEnumerable<Achievement> achievements,
    IContainer container,
    ModdableAchievementUnlocker unlocker,
    ModdableAchievementSpecService specs
) : IStoreAchievements
{
    internal static bool DisableSyncing = false;

    internal static Type? OriginalStoreAchievementType;

    IStoreAchievements? original;
    public readonly ImmutableArray<Achievement> Achievements = [.. achievements];

    public bool Initialized { get; private set; }

    public void Initialize(Action successCallback)
    {
        if (OriginalStoreAchievementType is null
            || (original = (IStoreAchievements)container.GetInstance(OriginalStoreAchievementType)) is null)
        {
            Done();
            return;
        }

        original.Initialize(() =>
        {
            SyncStoreUnlocked();
            Done();
        });

        void Done()
        {
            Validate();

            Initialized = true;
            successCallback();
        }
    }

    public bool IsAchievementUnlocked(string achievementId) => unlocker.IsUnlocked(achievementId);

    void SyncStoreUnlocked()
    {
        if (DisableSyncing || original is null) { return; }

        List<string> ids = [];
        foreach (var achievement in Achievements)
        {
            if (original.IsAchievementUnlocked(achievement.Id))
            {
                ids.Add(achievement.Id);
            }
        }

        if (ids.Count > 0) { unlocker.Unlock(ids); }
    }

    void Validate()
    {
        HashSet<string> checkedIds = [];
        var specKeys = specs.AchievementsByIds.Keys.ToHashSet();

        foreach (var ach in Achievements)
        {
            if (!specKeys.Contains(ach.Id))
            {
                Debug.LogWarning($"[{nameof(ModdableTimberbornAchievements)}] Achievement with ID '{ach.Id}' does not have an associated specs.");
            }

            checkedIds.Add(ach.Id);
        }

        foreach (var spec in specKeys)
        {
            if (!checkedIds.Contains(spec))
            {
                throw new InvalidOperationException($"Achievement spec with ID '{spec}' does not have an associated Achievement registered.");
            }
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        unlocker.Unlock([achievementId]);
        original?.UnlockAchievement(achievementId);
    }

}
