namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class DemolishResourcesAchievement(EventBus eb) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.DemolishResources";
    public override string Id => AchId;

    public const int DemolishCount = 100;

    int currFrame = -1;
    int countSoFar;

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent e)
    {
        var stack = e.Entity.GetComponent<RecoveredGoodStack>();
        if (stack is null) { return; }

        var frame = Time.frameCount;
        if (frame != currFrame)
        {
            currFrame = frame;
            countSoFar = 0;
        }

        var stockCount = stack.Inventory.Stock.Sum(g => g.Amount);
        countSoFar += stockCount;
        
        if (countSoFar >= DemolishCount)
        {
            Unlock();
        }
    }
}
