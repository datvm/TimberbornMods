namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class SingleWaterBodyAchievement(
    EventBus eb,
    IThreadSafeWaterMap waterMap,
    MapIndexService mapIndexService
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.SingleWaterBody";
    const float MinDepth = 1f - .01f;

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
        HashSet<int>? validHeights = null;
        HashSet<int> currHeights = [];

        var (sx, sy, _) = mapIndexService.TotalSize;

        var columns = waterMap.WaterColumns;
        var columnCounts = waterMap.ColumnCounts;

        // Greedy: check the 4 corners first since they are likely to fail first
        if (!ValidateCell(0, 0)) { return false; }
        if (!ValidateCell(sx - 1, 0)) { return false; }
        if (!ValidateCell(0, sy - 1)) { return false; }
        if (!ValidateCell(sx - 1, sy - 1)) { return false; }

        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                if (!ValidateCell(x, y)) { return false; }
            }
        }

        return true;

        bool ValidateCell(int x, int y)
        {
            FillHeightsWithWater(x, y, currHeights);
            if (currHeights.Count == 0) { return false; }

            if (validHeights is null)
            {
                validHeights = [.. currHeights];
            }
            else
            {
                validHeights.IntersectWith(currHeights);
                if (validHeights.Count == 0) { return false; }
            }

            return true;
        }

        void FillHeightsWithWater(int x, int y, HashSet<int> heights)
        {
            var cellIndex2D = mapIndexService.CellToIndex(new(x, y));
            heights.Clear();

            var count = columnCounts[cellIndex2D];
            for (int z = 0; z < count; z++)
            {
                var column = columns[cellIndex2D + z];
                var depth = column.WaterDepth;
                if (depth < MinDepth) { continue; }

                var curr = column.Floor;
                while (depth >= MinDepth)
                {
                    heights.Add(curr);
                    curr++;
                    depth--;
                }
            }
        }
    }
}
