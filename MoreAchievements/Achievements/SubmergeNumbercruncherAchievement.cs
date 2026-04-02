namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class SubmergeNumbercruncherAchievement(DefaultEntityTracker<NumbercruncherSubmergeComponent> tracker) : Achievement
{
    public const string AchId = "LV.MA.SubmergeNumbercruncher";

    public override string Id => AchId;

    public override void EnableInternal()
    {
        base.EnableInternal();

        tracker.OnEntityRegistered += OnEntityRegistered;
        foreach (var e in tracker.Entities)
        {
            OnEntityRegistered(e);
        }
    }

    void OnEntityRegistered(NumbercruncherSubmergeComponent obj)
    {
        obj.OnSubmerged += OnSubmerged;
    }

    void OnSubmerged(object sender, EventArgs e) => Unlock();

    public override void DisableInternal()
    {
        base.DisableInternal();

        tracker.OnEntityRegistered -= OnEntityRegistered;
        foreach (var e in tracker.Entities)
        {
            e.OnSubmerged -= OnSubmerged;
        }
    }

}
