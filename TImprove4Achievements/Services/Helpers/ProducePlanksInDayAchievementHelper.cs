namespace TImprove4Achievements.Services.Helpers;

public class ProducePlanksInDayAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<ProducePlanksInDayAchievement>("LV.T4A.ProducePlanksInDay", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [
        Achievement._planksProduced,
        ProducePlanksInDayAchievement.PlanksToProducePerDay
    ];
}