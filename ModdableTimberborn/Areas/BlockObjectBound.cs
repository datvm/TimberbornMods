namespace ModdableTimberborn.Areas;

public class BlockObjectBound(AreaSegmentService areaSegmentService) : BaseComponent, IAwakableComponent
{
    static readonly Vector3Int Default = new(-1, -1, -1);

    Vector3Int cachedPosition = Default;
    BoundsInt cachedBounds;
    ImmutableArray<int> segments = [];

    public BoundsInt Bounds
    {
        get
        {
            EnsureCached();
            return cachedBounds;
        }
    }

    public ImmutableArray<int> Segments
    {
        get
        {
            EnsureCached();
            return segments;
        }
    }

    public BlockObject BlockObject { get; private set; } = null!;

    public void Awake()
    {
        BlockObject = GetComponent<BlockObject>();
    }

    void EnsureCached()
    {
        if (BlockObject.Coordinates == cachedPosition) { return; }

        cachedPosition = BlockObject.Coordinates;
        cachedBounds = BlockObject.GetBounds();
        segments = [.. areaSegmentService.GetSegments(cachedBounds)];
    }

}
