namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class NoFloodForCyclesAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<NoFloodForCyclesAchievement>("LV.MA.NoFloodForCycles", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [Achievement.NoFloodCycles, NoFloodForCyclesAchievement.RequiredCycles];
}
