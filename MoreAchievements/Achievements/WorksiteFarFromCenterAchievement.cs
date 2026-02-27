namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class WorksiteFarFromCenterAchievement(EventBus eb) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.WorksiteFarFromCenter";
    public const int RequiredDistance = 150;

    public override string Id => AchId;

    [OnEvent]
    public void OnEntityFinished(EnteredFinishedStateEvent e)
    {
        try
        {
            var bo = e.BlockObject;
            if (!bo.GetComponent<Workplace>()) { return; }

            var dbd = bo.GetComponent<DistrictBuildingDistance>();
            if (!dbd) { return; }

            if (dbd.TryGetDistanceToDistrict(out var d))
            {
                if (d >= RequiredDistance)
                {
                    Unlock();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error when getting distance: " + ex);
        }
    }

}
