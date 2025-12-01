namespace TImprove4Achievements.Services.Helpers;

public class BuildManyHedgesAchievementHelper(DialogService diag, ILoc t) 
    : BaseMessageBoxAchievementHelper<BuildManyHedgesAchievement>("LV.T4A.BuildManyHedges", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [
        Achievement._hedgeCount,
        BuildManyHedgesAchievement.HedgesRequired
    ];
}