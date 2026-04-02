namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class WonderInADayAchievement(AchievementWonderService service, EventBus? eb = null) : BaseWonderLaunchAchievement(service, eb)
{
    public const string AchId = "LV.MA.WonderInADay";

    public override string Id => AchId;
    public override bool CanBeAchieved => true;

    public override bool ShouldUnlock(Wonder launchedWonder)
    {
        var tracker = launchedWonder.GetComponent<WonderCompletionTracker>();
        return tracker && tracker.LaunchedOnTheSameDay;
    }
}
