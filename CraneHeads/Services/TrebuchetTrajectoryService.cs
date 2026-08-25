namespace CraneHeads.Services;

[BindSingleton]
public class TrebuchetTrajectoryService(
    IBlockService blockService,
    ITerrainService terrain,
    MapSize mapSize
)
{
    readonly HashSet<Vector3Int> cells = [];
    readonly BresenhamLineDrawer drawer = new();

    public int MaxRange(CraneTower tower) => tower.Sections.Count;

    public int HorizontalDistance(Vector3Int from, Vector3Int to)
        => Math.Max(Math.Abs(to.x - from.x), Math.Abs(to.y - from.y));

    public bool InRange(Vector3Int from, Vector3Int to, int maxRange)
        => maxRange > 0
            && HorizontalDistance(from, to) is var d
            && d > 0
            && d <= maxRange;

    public bool TryGetPath(Vector3Int start, Vector3Int end, int peakDelta, List<Vector3Int> path)
    {
        path.Clear();
        CollectPath(start, end, peakDelta);
        foreach (var cell in cells)
        {
            path.Add(cell);
        }

        return cells.Count > 0;
    }

    public bool IsPathClear(Vector3Int start, Vector3Int end, int peakDelta, BlockObject? ignoring)
    {
        CollectPath(start, end, peakDelta);
        foreach (var cell in cells)
        {
            if (cell == start || cell == end)
            {
                continue;
            }

            if (!terrain.Contains(cell.XY()))
            {
                return false;
            }

            if (cell.z < 0)
            {
                return false;
            }

            if (cell.z >= mapSize.TotalSize.z)
            {
                continue;
            }

            if (terrain.Underground(cell))
            {
                return false;
            }

            if (IsBlocked(cell, ignoring))
            {
                return false;
            }
        }

        return true;
    }

    void CollectPath(Vector3Int start, Vector3Int end, int peakDelta)
    {
        cells.Clear();
        var dx = end.x - start.x;
        var dy = end.y - start.y;
        var steps = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), 1);
        var samples = Math.Max(steps * 2, 4);

        var prev = start;
        for (var i = 1; i <= samples; i++)
        {
            var t = i / (float)samples;
            var point = new Vector3Int(
                start.x + (int)Math.Round(t * dx),
                start.y + (int)Math.Round(t * dy),
                (int)Math.Round(ParabolaZ(t, start.z, end.z, peakDelta)));
            drawer.DrawLine(prev, point, cells);
            prev = point;
        }
    }

    bool IsBlocked(Vector3Int cell, BlockObject? ignoring)
    {
        if (!blockService.Contains(cell))
        {
            return false;
        }

        if (!blockService.AnyNonOverridableObjectsAt(cell, BlockOccupations.All))
        {
            return false;
        }

        if (ignoring is null)
        {
            return true;
        }

        foreach (var obj in blockService.GetObjectsAt(cell))
        {
            if (obj == ignoring)
            {
                continue;
            }

            if (obj.PositionedBlocks.GetBlock(cell).Occupation != BlockOccupations.None)
            {
                return true;
            }
        }

        return false;
    }

    static float ParabolaZ(float t, int z0, int z1, int peakDelta)
    {
        var dZ = z1 - z0;
        var a = 2 * dZ - 4 * peakDelta;
        var b = 4 * peakDelta - dZ;
        return a * t * t + b * t + z0;
    }
}
