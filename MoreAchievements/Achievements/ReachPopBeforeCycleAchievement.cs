namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class ReachPopBeforeCycleAchievement(
    GameCycleService cycle,
    EventBus eb,
    BeaverPopulation beaverPopulation
) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.ReachPopBeforeCycle";
    public const int RequiredBeavers = 200;
    public const int BeforeCycle = 5;

    public override string Id => AchId;

    public bool CanAchieve => cycle.Cycle < BeforeCycle;

    public override void EnableInternal()
    {
        if (!CanAchieve)
        {
            Disable();
            return;
        }

        base.EnableInternal();
    }

    [OnEvent]
    public void OnNewCycle(CycleStartedEvent _)
    {
        if (!CanAchieve)
        {
            Disable();
        }
    }

    [OnEvent]
    public void OnNewbornSpawn(BeaverBornEvent _)
    {
        if (CanAchieve && beaverPopulation.NumberOfBeavers >= RequiredBeavers)
        {
            Unlock();
        }
    }

}
