namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class TelescopeAtBottomAchievement : Achievement
{
    public static string AchId = "LV.MA.TelescopeAtBottom";
    public override string Id => AchId;
}
