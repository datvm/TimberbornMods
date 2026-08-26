namespace CraneHeads.Services;

[BindSingleton]
public class TrebuchetTrajectoryService(
    IBlockService blockService,
    ITerrainService terrain,
    MapSize mapSize,
    BlockValidator blockValidator,
    RecoveredGoodStackFactory recoveredGoodStacks
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

    public TrebuchetShotCheck Evaluate(Vector3Int start, Vector3Int end, int maxRange, int peakDelta, BlockObject? ignoring)
    {
        var distance = HorizontalDistance(start, end);
        if (distance <= 0)
        {
            return new(TrebuchetShotStatus.SameTile, 0, maxRange);
        }

        if (maxRange <= 0 || distance > maxRange)
        {
            return new(TrebuchetShotStatus.OutOfRange, distance, maxRange);
        }

        if (end.z > start.z + peakDelta)
        {
            return new(TrebuchetShotStatus.TooHigh, distance, maxRange);
        }

        if (!IsLandingValid(end))
        {
            return new(TrebuchetShotStatus.InvalidLanding, distance, maxRange);
        }

        if (!IsPathClear(start, end, peakDelta, ignoring))
        {
            return new(TrebuchetShotStatus.Blocked, distance, maxRange);
        }

        return new(TrebuchetShotStatus.Valid, distance, maxRange);
    }

    public void FillWorldPath(Vector3Int start, Vector3Int end, int peakDelta, List<Vector3> path, int samples = 24)
    {
        path.Clear();
        var count = Math.Max(samples, 2);
        for (var i = 0; i < count; i++)
        {
            path.Add(WorldPoint(start, end, peakDelta, i / (float)(count - 1)));
        }
    }

    public Vector3 WorldPoint(Vector3Int start, Vector3Int end, int peakDelta, float t)
    {
        t = Mathf.Clamp01(t);
        var grid = new Vector3(
            start.x + 0.5f + t * (end.x - start.x),
            start.y + 0.5f + t * (end.y - start.y),
            ParabolaZ(t, start.z, end.z, peakDelta) + 0.5f);
        return CoordinateSystem.GridToWorld(grid);
    }

    public Vector3 InitialWorldDirection(Vector3Int start, Vector3Int end, int peakDelta)
    {
        var dZ = end.z - start.z;
        var grid = new Vector3(
            end.x - start.x,
            end.y - start.y,
            4 * peakDelta - dZ);
        return CoordinateSystem.GridToWorld(grid);
    }

    public bool IsLandingValid(Vector3Int dest)
    {
        if (blockService.GetObjectsWithComponentAt<RecoveredGoodStack>(dest).Any())
        {
            return true;
        }

        var block = Block.From(dest, recoveredGoodStacks.GoodStackBlockSpec);
        return blockValidator.BlockValidWithoutUnfinishedStackable(block);
    }

    public bool IsPathClear(Vector3Int start, Vector3Int end, int peakDelta, BlockObject? ignoring)
        => FillBlockingCells(start, end, peakDelta, ignoring, null);

    public bool FillBlockingCells(
        Vector3Int start,
        Vector3Int end,
        int peakDelta,
        BlockObject? ignoring,
        List<Vector3Int>? blockers)
    {
        blockers?.Clear();
        CollectPath(start, end, peakDelta);
        var clear = true;
        foreach (var cell in cells)
        {
            if (cell == start || cell == end)
            {
                continue;
            }

            if (!terrain.Contains(cell.XY()) || cell.z < 0)
            {
                clear = false;
                continue;
            }

            if (cell.z >= mapSize.TotalSize.z)
            {
                continue;
            }

            if (terrain.Underground(cell) || IsBlocked(cell, ignoring))
            {
                clear = false;
                blockers?.Add(cell);
            }
        }

        return clear;
    }

    void CollectPath(Vector3Int start, Vector3Int end, int peakDelta)
    {
        cells.Clear();
        var dx = end.x - start.x;
        var dy = end.y - start.y;
        var steps = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Abs(end.z - start.z));
        var samples = Math.Max(steps * 4, 16);
        Vector3Int? prev = null;
        for (var i = 0; i <= samples; i++)
        {
            var t = i / (float)samples;
            var grid = new Vector3(
                start.x + 0.5f + t * dx,
                start.y + 0.5f + t * dy,
                ParabolaZ(t, start.z, end.z, peakDelta) + 0.5f);
            var point = new Vector3Int(
                (int)Math.Floor(grid.x),
                (int)Math.Floor(grid.y),
                (int)Math.Floor(grid.z));
            if (prev is { } last)
            {
                drawer.DrawLine(last, point, cells);
            }
            else
            {
                cells.Add(point);
            }

            prev = point;
        }
    }

    bool IsBlocked(Vector3Int cell, BlockObject? ignoring)
    {
        if (!blockService.AnyObjectAt(cell))
        {
            return false;
        }

        foreach (var obj in blockService.GetObjectsAt(cell))
        {
            if (obj != ignoring)
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
