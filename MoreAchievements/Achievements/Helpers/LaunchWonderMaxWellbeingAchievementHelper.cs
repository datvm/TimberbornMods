namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class LaunchWonderMaxWellbeingAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<LaunchWonderMaxWellbeingAchievement>("LV.MA.LaunchWonderMaxWellbeing", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [
        Achievement.WellbeingHighscore.ToString().Color(Achievement.Available ? TimberbornTextColor.Green : TimberbornTextColor.Red),
        LaunchWonderMaxWellbeingAchievement.MaxWellbeing];
}
