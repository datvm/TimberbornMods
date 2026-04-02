namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class StackResidentialBuildingsAchievement(
    EventBus eb,
    BuildingStackService buildingStackService
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.StackResidentialBuildings";
    public const int RequiredStack = 5;

    public override string Id => AchId;

    [OnEvent]
    public void OnBuildingFinished(EnteredFinishedStateEvent e)
    {
        if (Validate(e.BlockObject))
        {
            Unlock();
        }
    }

    bool Validate(BlockObject bo)
    {
        if (!bo.HasComponent<DwellingSpec>()) { return false; }

        var stack = buildingStackService.GetStackedBuildings(bo, RequiredStack, b =>
        {
            var db = b.GetComponent<DistrictBuilding>();
            if (!db || !db.District) { return false; }

            return true;
        });

        return stack.Count >= RequiredStack;
    }

}
