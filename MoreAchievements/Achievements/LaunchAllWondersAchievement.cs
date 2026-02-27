
namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchAllWondersAchievement(
    AchievementWonderService service,
    EventBus eb
) : BaseWonderLaunchAchievement(service, eb)
{
    public static string AchId = "LV.MA.LaunchAllWonders";
    public override string Id => AchId;

    public override bool CanBeAchieved => service.WonderCount > 1;
    readonly HashSet<string> launchedTemplates = [];
    public IReadOnlyCollection<string> LaunchedTemplate => launchedTemplates;

    public override bool ShouldUnlock(Wonder launchedWonder)
    {
        var template = launchedWonder.GetTemplateName();
        launchedTemplates.Add(template);

        return launchedTemplates.Count >= service.WonderCount;
    }

    [OnEvent]
    public void OnNewCycle(CycleStartedEvent e)
    {
        launchedTemplates.Clear();
    }

}
