namespace Crane;

public static class CraneUtils
{

    extension(BlockObject bo)
    {
        // Simpler than ConstructionSite restricted BaseZ. We just take the whole box of the building.
        public BoundsInt GetConstructionBounds()
        {
            Vector3Int? min = null;
            var max = Vector3Int.zero;
            foreach (var cell in bo.PositionedBlocks.GetOccupiedCoordinates())
            {
                if (min is null)
                {
                    min = max = cell;
                    continue;
                }

                min = Vector3Int.Min(min.Value, cell);
                max = Vector3Int.Max(max, cell);
            }

            var origin = min ?? bo.Coordinates;
            return new(origin, max - origin + Vector3Int.one);
        }
    }

    extension(BoundsInt bounds)
    {
        public bool Overlaps(BoundsInt other)
            => bounds.xMin < other.xMax
               && bounds.xMax > other.xMin
               && bounds.yMin < other.yMax
               && bounds.yMax > other.yMin
               && bounds.zMin < other.zMax
               && bounds.zMax > other.zMin;
    }

}
