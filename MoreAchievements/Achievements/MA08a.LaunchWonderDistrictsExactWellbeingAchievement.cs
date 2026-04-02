namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchWonderDistrictsExactWellbeingAchievement(
    DistrictCenterRegistry districtCenterRegistry,
    AchievementWonderService service
) : BaseWonderLaunchAchievement(service)
{
    public static string AchId = "LV.MA.LaunchWonderDistrictsExactWellbeing";
    public const int RequiredWellbeing = 77;
    public const int DistrictCount1 = 3;
    public const int DistrictCount2 = 7;

    public override string Id => AchId;

    public override bool CanBeAchieved => true;

    public override bool ShouldUnlock(Wonder? _ = null)
    {
        var dcs = districtCenterRegistry.AllDistrictCenters;
        var count = dcs.Count;
        if (count != DistrictCount1 && count != DistrictCount2) { return false; }

        foreach (var (_, w) in GetDistrictWellbeings())
        {
            if (w != RequiredWellbeing) { return false; }
        }

        return true;
    }

    public IEnumerable<(DistrictCenter, int)> GetDistrictWellbeings()
    {
        foreach (var dc in districtCenterRegistry.AllDistrictCenters)
        {
            yield return (dc, dc.GetComponent<DistrictWellbeingTrackerRegistry>().Registry.GetAverageWellbeing());
        }
    }

}
