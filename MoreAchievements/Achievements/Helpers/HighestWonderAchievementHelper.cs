namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class HighestWonderAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<HighestWonderAchievement>("LV.MA.HighestWonder", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [Achievement.RequiredHeight];
}
