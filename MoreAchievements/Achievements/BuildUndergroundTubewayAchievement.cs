namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class BuildUndergroundTubewayAchievement(
    EventBus eb,
    IBlockService blocks,
    TerrainMap terrainMap
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.BuildUndergroundTubeway";
    public const int RequiredLength = 10;

    public override string Id => AchId;

    [OnEvent]
    public void OnBuildingFinished(EnteredFinishedStateEvent e)
    {
        var bo = e.BlockObject;
        if (!bo.HasComponent<TubeStationSpec>()) { return; }

        if (FindUndergroundLength(bo) >= RequiredLength)
        {
            Unlock();
        }
    }

    int FindUndergroundLength(BlockObject starting)
    {
        HashSet<Vector3Int> visitedCoords = [];
        Stack<Vector3Int> toVisit = [];
        foreach (var b in starting.PositionedBlocks.GetAllBlocks())
        {
            visitedCoords.Add(b.Coordinates);
        }
        foreach (var b in starting.PositionedBlocks.GetAllBlocks())
        {
            PushNeighbors(b.Coordinates, toVisit, visitedCoords);
        }

        var hasExit = false;
        var counter = 0;

        while (toVisit.Count > 0)
        {
            var c = toVisit.Pop();

            var objs = blocks.GetObjectsAt(c);
            foreach (var obj in objs)
            {
                if (obj.HasComponent<TubeStationSpec>() && obj != starting && obj.IsFinished)
                {
                    hasExit = true;
                }
                else if (obj.HasComponent<TubeSpec>() && obj.IsFinished)
                {
                    var hasDirtAbove = terrainMap.IsTerrainVoxel(c with { z = c.z + 1, });
                    if (hasDirtAbove)
                    {
                        counter++;
                    }
                    else
                    {
                        return 0;
                    }

                    PushNeighbors(c, toVisit, visitedCoords);
                }
            }
        }

        return hasExit ? counter : 0;
    }

    void PushNeighbors(Vector3Int center, Stack<Vector3Int> stack, HashSet<Vector3Int> marker)
    {
        foreach (var n in Deltas.Neighbors4Vector3Int)
        {
            var c = center + n;
            if (marker.Add(c))
            {
                stack.Push(c);
            }
        }
    }

}
