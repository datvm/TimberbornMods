namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchWonderMaxWellbeingAchievement(
    AchievementWonderService service,
    WellbeingHighscore wellbeingHighscore,
    EventBus eb
) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.LaunchWonderMaxWellbeing";
    public override string Id => AchId;

    public const int MaxWellbeing = 2;
    public bool Available => WellbeingHighscore <= MaxWellbeing;
    public int WellbeingHighscore => wellbeingHighscore._averageWellbeingHighscore;

    public override void EnableInternal()
    {
        if (!Available)
        {
            Disable();
            return;
        }

        base.EnableInternal();
        service.OnWonderLaunched += OnWonderLaunched;
    }

    public override void DisableInternal()
    {
        base.DisableInternal();
        service.OnWonderLaunched -= OnWonderLaunched;
    }

    void OnWonderLaunched(Wonder obj)
    {
        if (Available)
        {
            Unlock();
        }
    }

    [OnEvent]
    public void OnNewHighscore(NewWellbeingHighscoreEvent _)
    {
        if (!Available)
        {
            Disable();
        }
    }

}
