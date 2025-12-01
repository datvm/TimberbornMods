
namespace TImprove4Achievements.Services.Helpers;

public class WorkAllDayForWeekAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<WorkAllDayForWeekAchievement>("LV.T4A.WorkAllDay", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [
        WorkAllDayForWeekAchievement.WorkingDaysRequired - Achievement._timeTrigger.DaysLeft,
        WorkAllDayForWeekAchievement.WorkingDaysRequired
    ];
}
