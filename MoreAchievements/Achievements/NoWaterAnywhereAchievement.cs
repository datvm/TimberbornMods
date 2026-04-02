namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class NoWaterAnywhereAchievement(
    EventBus eb,
    IThreadSafeWaterMap waterMap
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.NoWaterAnywhere";
    public override string Id => AchId;

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        if (Validate())
        {
            Unlock();
        }
    }

    bool Validate()
    {
        var columns = waterMap.WaterColumns;
        foreach (var c in columns.AsSpan)
        {
            if (c.WaterDepth > 0)
            {
                return false;
            }
        }
        
        return true;
    }
}
