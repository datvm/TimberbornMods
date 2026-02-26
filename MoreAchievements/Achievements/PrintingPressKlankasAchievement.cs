namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class PrintingPressKlankasAchievement : Achievement
{
    public static string AchId = "LV.MA.PrintingPressKlankas";
    public override string Id => AchId;
}
