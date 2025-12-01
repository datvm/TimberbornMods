namespace TImprove4Achievements.Services.Helpers;

public class ZiplineNetworkLengthAchievementHelper(
    DialogService diag,
    ILoc t,
    EntityRegistry entities
)
    : BaseMessageBoxAchievementHelper<ZiplineNetworkLengthAchievement>("LV.T4A.ZiplineNetworkLength", 1, diag, t)
{
    protected override object[] GetParameters(int step)
    {
        return [
            GetMaxCableNetworkLength(),
            ZiplineNetworkLengthAchievement.MinimumLength,
        ];
    }

    float GetMaxCableNetworkLength()
    {
        var max = 0f;
        HashSet<ZiplineTower> visited = [];

        foreach (var e in entities.Entities)
        {
            var ziplineTower = e.GetComponent<ZiplineTower>();
            if (!ziplineTower || visited.Contains(ziplineTower)) { continue; }

            var num = GetCableNetworkLength(Achievement, ziplineTower, visited);
            if (num > max)
            {
                max = num;
            }
        }

        return max;
    }

    static float GetCableNetworkLength(ZiplineNetworkLengthAchievement ach, ZiplineTower startTower, HashSet<ZiplineTower> visited)
    {
        ach._towersToVisit.Enqueue(startTower);
        while (ach._towersToVisit.Count > 0)
        {
            ZiplineTower ziplineTower = ach._towersToVisit.Dequeue();
            visited.Add(ziplineTower);

            if (!ach._visitedTowers.Add(ziplineTower))
            {
                continue;
            }
            foreach (ZiplineTower connectionTarget in ziplineTower.ConnectionTargets)
            {
                ach._visitedEdges.Add((connectionTarget, ziplineTower));
                if (!ach._visitedTowers.Contains(connectionTarget))
                {
                    ach._towersToVisit.Enqueue(connectionTarget);
                }
            }
        }
        float num = 0f;
        foreach (var visitedEdge in ach._visitedEdges)
        {
            ZiplineTower item = visitedEdge.Item1;
            ZiplineTower item2 = visitedEdge.Item2;
            num += Vector3.Distance(item.CableAnchorPoint, item2.CableAnchorPoint);
        }
        ach._visitedTowers.Clear();
        ach._towersToVisit.Clear();
        ach._visitedEdges.Clear();

        return num;
    }

}
