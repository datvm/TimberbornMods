namespace TImprove4Achievements.Services;

public class AchievementHelperService(
    IEnumerable<Achievement> achievements,
    IEnumerable<BaseAchievementHelper> helpers,
    ModdableAchievementSpecService specs
) : ILoadableSingleton
{

    public readonly FrozenDictionary<Type, Achievement> AchievementsByType = achievements.ToFrozenDictionary(a => a.GetType());
    public FrozenDictionary<string, BaseAchievementHelper> HelpersByIds { get; private set; } = FrozenDictionary<string, BaseAchievementHelper>.Empty;

    public void Load()
    {
        Dictionary<string, BaseAchievementHelper> helpersByIds = [];

        foreach (var h in helpers)
        {
            var ach = AchievementsByType[h.AchievementType];
            h.Initialize(ach, specs.AchievementsByIds[ach.Id]);

            if (h.StepsCount <= 0)
            {
                throw new ArgumentException($"Achievement helper {h.GetType().Name} for achievement {ach.Id} has invalid steps count {h.StepsCount}");
            }

            helpersByIds.Add(ach.Id, h);
        }

        HelpersByIds = helpersByIds.ToFrozenDictionary();
    }

}