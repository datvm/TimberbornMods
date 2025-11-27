namespace ModdableTimberbornAchievements.Services;

public class ModdableAchievementSpecService(ISpecService specs) : ILoadableSingleton
{

    public ImmutableArray<ModdableAchievementGroupSpec> AchievementGroups { get; private set; } = [];
    public FrozenDictionary<string, ModdableAchievementGroupSpec> AchievementGroupsByIds { get; private set; } = FrozenDictionary<string, ModdableAchievementGroupSpec>.Empty;

    public ImmutableArray<ModdableAchievementSpec> Achievements { get; private set; } = [];
    public FrozenDictionary<string, ModdableAchievementSpec> AchievementsByIds { get; private set; } = FrozenDictionary<string, ModdableAchievementSpec>.Empty;

    public FrozenDictionary<string, ImmutableArray<ModdableAchievementSpec>> AchievementsByGroupIds { get; private set; } = FrozenDictionary<string, ImmutableArray<ModdableAchievementSpec>>.Empty;

    public void Load()
    {
        AchievementGroups = [.. specs.GetSpecs<ModdableAchievementGroupSpec>().OrderBy(q => q.Order)];
        AchievementGroupsByIds = AchievementGroups.ToFrozenDictionary(q => q.Id);

        Dictionary<string, ModdableAchievementSpec> achievementSpecs = [];
        Dictionary<string, List<ModdableAchievementSpec>> achievementsByGroupIds = [];
        Achievements = [.. specs.GetSpecs<ModdableAchievementSpec>().OrderBy(q => q.Order)];
        foreach (var ach in Achievements)
        {
            var grpId = ach.GroupId;
            if (!AchievementGroupsByIds.ContainsKey(grpId))
            {
                throw new Exception($"Achievement '{ach.Id}' references unknown group id '{grpId}'");
            }

            var id = ach.Id;

            achievementSpecs[id] = ach;
            achievementsByGroupIds.GetOrAdd(grpId).Add(ach);
        }

        AchievementsByIds = achievementSpecs.ToFrozenDictionary();
        AchievementsByGroupIds = achievementsByGroupIds.ToFrozenDictionary(q => q.Key, q => q.Value.ToImmutableArray());
    }

}
