namespace TImprove4Achievements.Services.Helpers;

public class ExplodeDynamiteInSingleDayAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<ExplodeDynamiteInSingleDayAchievement>("LV.T4A.ExplodeDynamiteInSingleDay", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [
        Achievement._detonationCount,
        ExplodeDynamiteInSingleDayAchievement.DynamiteToDetonate
    ];

}
