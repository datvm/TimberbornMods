namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class ReachPopBeforeCycleAchievement : Achievement
{
    public static string AchId = "LV.MA.ReachPopBeforeCycle";
    public override string Id => AchId;
}
