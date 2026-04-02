namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class BuildZiplineLoopAchievement(EventBus eb) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.BuildZiplineLoop";
    public const int LoopCount = 8;

    public override string Id => AchId;
    

    [OnEvent]
    public void OnZiplineConnectionActivated(ZiplineConnectionActivatedEvent e)
    {
        var startingTower = e.ZiplineTower;

        if (ValidateLoop(startingTower))
        {
            Unlock();
        }
    }

    bool ValidateLoop(ZiplineTower startingTower)
    {
        if (!startingTower.HasComponent<ZiplineStationSpec>()) { return false; }

        HashSet<ZiplineTower> visited = [];
        ZiplineTower? prev = null;
        var curr = startingTower;
        var counter = 0;

        while (counter < LoopCount)
        {
            if (curr.ConnectionTargets.Count != 2) { return false; }

            var other = curr.ConnectionTargets[0] == prev ? curr.ConnectionTargets[1] : curr.ConnectionTargets[0];
            prev = curr;
            curr = other;
            visited.Add(prev);
            counter++;

            if (!curr.HasComponent<ZiplineStationSpec>()) { return false; }

            if (counter == LoopCount) // Check this BEFORE visit check because the last tower must be the starting tower.
            {
                return curr == startingTower;
            }
            else if (visited.Contains(curr))
            {
                return false;
            }
        }

        return false;
    }

}
