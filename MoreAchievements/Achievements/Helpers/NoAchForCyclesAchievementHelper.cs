namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class NoAchForCyclesAchievementHelper(DialogService diag, ILoc t) 
    : BaseMessageBoxAchievementHelper<NoAchForCyclesAchievement>("LV.MA.NoAchForCycles", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [Achievement.Cycles, NoAchForCyclesAchievement.RequiredCycles];
}
