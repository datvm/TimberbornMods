namespace TImprove4Achievements.Services.Helpers;

public abstract class PlugBadwaterSourceAchievementHelper<T>(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<T>("LV.T4A.PlugBadwaterSource", 1, diag, t)
    where T : PlugBadwaterSourceAchievement
{
    protected override object[] GetParameters(int step)
    {
        var count = Achievement._mustPlugAll ? Achievement._waterSources.Count : 1;
        var plugged = Achievement._waterSources.FastCount(PlugBadwaterSourceAchievement.IsPlugged);

        return [plugged, count];
    }
}

public class PlugAnyBadwaterSourceAchievementAchievementHelper(DialogService diag, ILoc t)
    : PlugBadwaterSourceAchievementHelper<PlugAnyBadwaterSourceAchievementAchievement>(diag, t)
{ }

public class PlugAllBadwaterSourcesAchievementAchievementHelper(DialogService diag, ILoc t)
    : PlugBadwaterSourceAchievementHelper<PlugAllBadwaterSourcesAchievementAchievement>(diag, t)
{ }