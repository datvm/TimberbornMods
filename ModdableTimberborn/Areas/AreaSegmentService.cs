namespace ModdableTimberborn.Areas;

public class AreaSegmentService(MapSize mapSize) : ILoadableSingleton
{
    public const int SegmentSize = 26;

    int sizeX, sizeY; // Z is typically small enough to not require segmentation
    int countX, countY, count;
    public ImmutableArray<RectInt> SegmentBounds { get; private set; } = [];

    public int HorizontalSegmentsCount => countX;
    public int VerticalSegmentsCount => countY;
    public int SegmentsCount => count;

    public void Load()
    {
        (sizeX, sizeY, _) = mapSize.TotalSize;
        countX = (sizeX + SegmentSize - 1) / SegmentSize;
        countY = (sizeY + SegmentSize - 1) / SegmentSize;

        List<RectInt> segments = [];
        for (int x = 0; x < sizeX; x += SegmentSize)
        {
            for (int y = 0; y < sizeY; y += SegmentSize)
            {
                segments.Add(new RectInt(x, y, SegmentSize, SegmentSize));
            }
        }

        SegmentBounds = [.. segments];
        count = segments.Count;
    }

    public int GetSegment(int x, int y)
    {
        var segX = x < 0 || x >= sizeX ? -1 : x / SegmentSize;
        if (segX < 0) { return -1; }

        var segY = y < 0 || y >= sizeY ? -1 : y / SegmentSize;
        if (segY < 0) { return -1; }

        return segX * countY + segY;
    }
    public int GetSegment(Vector2Int cell) => GetSegment(cell.x, cell.y);
    public int GetSegment(Vector3Int cell) => GetSegment(cell.x, cell.y);

    public IEnumerable<int> GetSegments(BoundsInt bounds)
    {
        var minCellX = Math.Max(bounds.xMin, 0);
        var maxCellX = Math.Min(bounds.xMax - 1, sizeX - 1);
        var minCellY = Math.Max(bounds.yMin, 0);
        var maxCellY = Math.Min(bounds.yMax - 1, sizeY - 1);

        if (minCellX > maxCellX || minCellY > maxCellY)
        {
            yield break;
        }

        var minX = minCellX / SegmentSize;
        var maxX = maxCellX / SegmentSize;
        var minY = minCellY / SegmentSize;
        var maxY = maxCellY / SegmentSize;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                yield return x * countY + y;
            }
        }
    }

    public HashSet<int> GetSegments(IEnumerable<BoundsInt> bounds)
    {
        var segments = new HashSet<int>();
        foreach (var b in bounds)
        {
            foreach (var s in GetSegments(b))
            {
                segments.Add(s);
            }
        }
        return segments;
    }
}