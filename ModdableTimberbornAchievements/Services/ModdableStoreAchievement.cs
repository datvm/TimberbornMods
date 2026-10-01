namespace ModdableTimberbornAchievements.Services;

public class ModdableStoreAchievement(
    IEnumerable<Achievement> achievements,
    IContainer container,
    ModdableAchievementUnlocker unlocker,
    ModdableAchievementSpecService specs
) : IStoreAchievements, ILoadableSingleton
{
    internal static Type? OriginalStoreAchievementType;

    IStoreAchievements? original;
    public readonly ImmutableArray<Achievement> Achievements = [.. achievements];

    public bool CanSync => original is not null && MStarter.HasSteam;

    public bool IsAchievementUnlocked(string achievementId) => unlocker.IsUnlocked(achievementId);

    public void Load()
    {
        original = ResolveOriginal(container, specs, achievements);
    }

    public void SyncStoreUnlocked()
    {
        if (original is null || !CanSync) { return; }

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

    static IStoreAchievements? ResolveOriginal(
        IContainer container,
        ModdableAchievementSpecService specs,
        IEnumerable<Achievement> achievements)
    {
        Validate(specs, achievements);

        var storeType = OriginalStoreAchievementType;
        if (storeType is null)
        {
            return null;
        }

        if (container.GetInstance(storeType) is not IStoreAchievements store)
        {
            throw new InvalidOperationException(
                $"[{nameof(ModdableTimberbornAchievements)}] '{storeType.FullName}' does not implement {nameof(IStoreAchievements)}.");
        }

        return store;
    }

    static void Validate(ModdableAchievementSpecService specs, IEnumerable<Achievement> achievements)
    {
        HashSet<string> checkedIds = [];
        var specKeys = specs.AchievementsByIds.Keys.ToHashSet();

        foreach (var ach in achievements)
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
        if (original is not null)
        {
            original.UnlockAchievement(achievementId);
            return;
        }

        if (MStarter.HasSteam)
        {
            throw new InvalidOperationException(
                $"[{nameof(ModdableTimberbornAchievements)}] Steam is loaded, but no achievement store was captured. Refusing to drop '{achievementId}'.");
        }
    }

}
