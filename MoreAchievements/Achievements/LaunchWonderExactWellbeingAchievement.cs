namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchWonderExactWellbeingAchievement(
    AchievementWonderService service,
    WellbeingService wellbeingService
)
    : BaseWonderLaunchAchievement(service)
{
    public static string AchId = "LV.MA.LaunchWonderExactWellbeing";
    public const int RequiredWellbeing = 77;

    public override string Id => AchId;

    public override bool CanBeAchieved => true;

    public override bool ShouldUnlock(Wonder launchedWonder)
        => wellbeingService.AverageGlobalWellbeing == RequiredWellbeing;
}
