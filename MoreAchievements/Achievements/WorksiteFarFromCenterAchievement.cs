namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class WorksiteFarFromCenterAchievement : Achievement
{
    public static string AchId = "LV.MA.WorksiteFarFromCenter";
    public override string Id => AchId;
}
