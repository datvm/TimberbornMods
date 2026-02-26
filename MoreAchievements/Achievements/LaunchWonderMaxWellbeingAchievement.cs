namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchWonderMaxWellbeingAchievement : Achievement
{
    public static string AchId = "LV.MA.LaunchWonderMaxWellbeing";
    public override string Id => AchId;
}
