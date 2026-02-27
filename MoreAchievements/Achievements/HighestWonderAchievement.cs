namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class HighestWonderAchievement(
    AchievementWonderService service,
    MapSize mapSize
) : BaseWonderLaunchAchievement(service)
{
    public const string AchId = "LV.MA.HighestWonder";

    public override string Id => AchId;

    public int RequiredHeight { get; private set; }

    public override bool CanBeAchieved => RequiredHeight > 0;
    public override bool ShouldUnlock(Wonder launchedWonder)
        => launchedWonder.GetComponent<BlockObject>().Coordinates.z >= RequiredHeight;

    public override void EnableInternal()
    {
        var wonders = service.WonderTemplates.Values;

        if (wonders.Length == 0) { return; }

        var minWonderHeight = wonders.Min(w => w.GetSpec<BlockObjectSpec>().Size.z);
        RequiredHeight = mapSize.TotalSize.z - minWonderHeight;

        base.EnableInternal();
    }

}
