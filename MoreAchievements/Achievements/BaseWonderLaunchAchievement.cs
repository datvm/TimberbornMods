namespace MoreAchievements.Achievements;

public abstract class BaseWonderLaunchAchievement(
    AchievementWonderService service,
    EventBus? eb = null
) : Achievement
{
    protected AchievementWonderService service = service;

    public abstract bool CanBeAchieved { get; }
    public abstract bool ShouldUnlock(Wonder launchedWonder);

    public override void EnableInternal()
    {
        if (!CanBeAchieved)
        {
            Disable();
            return;
        }

        eb?.Register(this);
        service.OnWonderLaunched += OnWonderLaunch;
    }

    public override void DisableInternal()
    {
        eb?.Unregister(this);
        service.OnWonderLaunched -= OnWonderLaunch;
    }

    protected void OnWonderLaunch(Wonder wonder)
    {
        if (ShouldUnlock(wonder))
        {
            Unlock();
        }
    }
}
