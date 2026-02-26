namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class OnlyEggplantsAchievement : Achievement
{
    public static string AchId = "LV.MA.OnlyEggplants";
    public override string Id => AchId;
}
