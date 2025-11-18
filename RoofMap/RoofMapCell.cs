namespace RoofMap;

/// <summary>
/// Contains roof map data for a single cell.
/// </summary>
public class RoofMapCell(int x, int y, int index2D)
{
    /// <summary>
    /// The X coordinate of this cell.
    /// </summary>
    public readonly int X = x;

    /// <summary>
    /// The Y coordinate of this cell.
    /// </summary>
    public readonly int Y = y;

    /// <summary>
    /// The 2D index of this cell.
    /// </summary>
    public readonly int Index2D = index2D;

    /// <summary>
    /// The highest terrain level of this cell.
    /// </summary>
    public int HighestTerrain { get; internal set; }

    /// <summary>
    /// The highest block object in this cell if any.
    /// </summary>
    public BlockObject? HighestBlockObject { get; private set; }
    /// <summary>
    /// The top of the highest block object in this cell if any.
    /// </summary>
    public int? HighestBlockObjectTop { get; private set; }

    /// <summary>
    /// The highest solid block object in this cell if any.
    /// </summary>
    public BlockObject? HighestSolidBlockObject { get; private set; }
    /// <summary>
    /// The top of the highest solid block object in this cell if any.
    /// </summary>
    public int? HighestSolidBlockObjectTop { get; private set; }

    internal bool TrySetHighestBlockObject(BlockObject bo, bool isSolid)
    {
        var top = FindObjectTop(bo, isSolid);
        if (top is null) { return false; }

        if (isSolid)
        {
            HighestSolidBlockObject = bo;
            HighestSolidBlockObjectTop = top;
        }
        else
        {
            HighestBlockObject = bo;
            HighestBlockObjectTop = top;
        }
        return true;
    }

    int? FindObjectTop(BlockObject? bo, bool solidOnly)
    {
        if (bo is null) { return null; }

        var highest = -1;

        foreach (var b in bo.PositionedBlocks._all)
        {
            var c = b.Coordinates;
            if (c.x != X || c.y != Y || highest >= c.z) { continue; }

            if (!solidOnly || b.Stackable == BlockStackable.BlockObject)
            {
                highest = c.z;
            }
        }

        return highest == -1 ? null : highest;
    }

}
