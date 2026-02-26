namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchWonderMinWellbeingAchievement : Achievement
{
    public static string AchId = "LV.MA.LaunchWonderMinWellbeing";
    public override string Id => AchId;
}
