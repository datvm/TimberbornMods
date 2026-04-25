namespace RadialToolbar.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarSegmentProvider(MSettings s)
{
    Rect cachedRect;
    int cachedSegmentCount;
    ImmutableArray<ToolbarSegment> cachedSegments = [];
    bool hasCache;

    public IReadOnlyList<ToolbarSegment> GetSegments(Rect rect)
    {
        var segmentCount = s.SegmentCount;

        if (hasCache && cachedRect == rect && cachedSegmentCount == segmentCount)
        {
            return cachedSegments;
        }

        var center = rect.center;
        var stepDeg = 360f / segmentCount;

        // This makes the segment centers line up with W/A/S/D.
        var startAngleDeg = -90f - stepDeg * 0.5f;

        var segments = new ToolbarSegment[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            var a0 = startAngleDeg + i * stepDeg;
            var a1 = a0 + stepDeg;

            var d0 = DirFromDegrees(a0);
            var d1 = DirFromDegrees(a1);

            var hit0 = RayToRectEdge(center, d0, rect);
            var hit1 = RayToRectEdge(center, d1, rect);

            segments[i] = new ToolbarSegment(
                i,
                center,
                a0,
                a1,
                new ToolbarRay(center, hit0.Point, d0, a0, hit0.Side),
                new ToolbarRay(center, hit1.Point, d1, a1, hit1.Side),
                GetLocation(i, segmentCount)
            );
        }

        cachedRect = rect;
        cachedSegmentCount = segmentCount;
        cachedSegments = [..segments];
        hasCache = true;

        return cachedSegments;
    }

    public int? GetSegmentAt(Vector3 v)
    {
        if (!hasCache || cachedSegments.Length == 0)
        {
            return null;
        }

        var point = (Vector2)v;
        var center = cachedRect.center;
        var delta = point - center;

        // Optional: ignore the exact center if desired.
        // Remove this block if you want the center to still select something.
        if (delta.sqrMagnitude < 0.0001f)
        {
            return null;
        }

        var stepDeg = 360f / cachedSegmentCount;
        var startAngleDeg = cachedSegments[0].StartAngleDeg;

        var angleDeg = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        var normalized = Mathf.Repeat(angleDeg - startAngleDeg, 360f);

        var index = Mathf.FloorToInt(normalized / stepDeg);
        index = Mathf.Clamp(index, 0, cachedSegmentCount - 1);

        return cachedSegments[index].Index;
    }

    public ToolbarSegment GetSegment(int index)
    {
        if (!hasCache || cachedSegments.Length == 0)
        {
            throw new InvalidOperationException("No cached segments available. Call GetSegments(rect) first to populate the cache.");
        }

        if (index >= cachedSegments.Length)
        {
            throw new InvalidOperationException($"Requested segment index {index} is out of bounds for the current segment count {cachedSegments.Length}.");
        }

        return cachedSegments[index];
    }

    public void Invalidate()
    {
        hasCache = false;
        cachedSegments = [];
    }

    static Vector2 DirFromDegrees(float degrees)
    {
        var rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    static RayHit RayToRectEdge(Vector2 origin, Vector2 dir, Rect rect)
    {
        float bestT = float.PositiveInfinity;
        Vector2 bestPoint = default;
        Direction bestSide = Direction.Up;

        if (!Mathf.Approximately(dir.x, 0f))
        {
            var x = dir.x > 0f ? rect.xMax : rect.xMin;
            var t = (x - origin.x) / dir.x;
            var y = origin.y + dir.y * t;

            if (t > 0f && y >= rect.yMin - 0.001f && y <= rect.yMax + 0.001f && t < bestT)
            {
                bestT = t;
                bestPoint = new Vector2(x, y);
                bestSide = dir.x > 0f ? Direction.Right : Direction.Left;
            }
        }

        if (!Mathf.Approximately(dir.y, 0f))
        {
            var y = dir.y > 0f ? rect.yMax : rect.yMin;
            var t = (y - origin.y) / dir.y;
            var x = origin.x + dir.x * t;

            if (t > 0f && x >= rect.xMin - 0.001f && x <= rect.xMax + 0.001f && t < bestT)
            {
                bestT = t;
                bestPoint = new Vector2(x, y);
                bestSide = dir.y > 0f ? Direction.Down : Direction.Up;
            }
        }

        return new RayHit(bestPoint, bestSide);
    }

    static Direction GetLocation(int index, int segmentCount) => segmentCount switch
    {
        4 => index switch
        {
            0 => Direction.Up,
            1 => Direction.Right,
            2 => Direction.Down,
            3 => Direction.Left,
            _ => Direction.None,
        },

        8 => index switch
        {
            0 => Direction.Up,
            1 => Direction.Up | Direction.Right,
            2 => Direction.Right,
            3 => Direction.Right | Direction.Down,
            4 => Direction.Down,
            5 => Direction.Down | Direction.Left,
            6 => Direction.Left,
            7 => Direction.Left | Direction.Up,
            _ => Direction.None,
        },

        _ => Direction.None,
    };

    readonly record struct RayHit(Vector2 Point, Direction Side);

}