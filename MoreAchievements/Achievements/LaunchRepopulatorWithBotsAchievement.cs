namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchRepopulatorWithBotsAchievement : Achievement
{
    public static string AchId = "LV.MA.LaunchRepopulatorWithBots";
    public override string Id => AchId;
}
