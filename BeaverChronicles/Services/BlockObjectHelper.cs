namespace BeaverChronicles.Services;

[BindSingleton]
public class BlockObjectHelper
{

    public static bool IsIntersectingArea(BlockObject bo, BoundsInt bounds) 
        => IsInsideArea(bo, bounds, false);

    public static bool IsInsideArea(BlockObject bo, BoundsInt bounds) 
        => IsInsideArea(bo, bounds, true);

    public static bool IsInsideArea(BlockObject bo, BoundsInt bounds, bool all)
    {
        foreach (var b in bo.PositionedBlocks._all)
        {
            if (bounds.Contains(b.Coordinates))
            {
                if (!all) { return true; }
            }
            else
            {
                if (all) { return false; }
            }
        }

        return all;
    }

}
