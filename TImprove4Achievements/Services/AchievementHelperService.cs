namespace TImprove4Achievements.UI;

public class AchievementHelperService
{

    public readonly FrozenDictionary<Type, Achievement> AchievementsByType;
    public readonly ImmutableArray<BaseAchievementHelper> Helpers;

    public AchievementHelperService(
        IEnumerable<Achievement> achievements,
        IEnumerable<BaseAchievementHelper> helpers
    )
    {
        AchievementsByType = achievements.ToFrozenDictionary(a => a.GetType());
        Helpers = [..helpers.Select(h => {
            h.SetAchievement(AchievementsByType[h.AchievementType]);
            return h;
        })];
    }

}