namespace ModdableTimberborn.Helpers;

public static class AreaExtensions
{

    extension(BlockObject bo)
    {
        /// <summary>
        /// World-space half-open AABB of the block object's local size, accounting for
        /// orientation and flip. Do not use <c>Coordinates + Size</c> — the placement
        /// anchor is not always the min corner after rotation.
        /// </summary>
        public BoundsInt GetBounds()
        {
            var size = bo._blockObjectSpec.Size;
            // Inclusive local cells [0, size); opposite corners suffice for 90°/flip.
            var a = bo.TransformCoordinates(Vector3Int.zero);
            var b = bo.TransformCoordinates(size - Vector3Int.one);
            var min = Vector3Int.Min(a, b);
            var max = Vector3Int.Max(a, b);
            // BoundsInt is half-open [min, max); max above is inclusive.
            return new(min, max - min + Vector3Int.one);
        }
    }

    extension(AreaCondition condition)
    {

        public bool Evaluate(BoundsInt obj, BoundsInt area) => condition switch
        {
            AreaCondition.Intersects => obj.Intersects(area),
            AreaCondition.Contains => area.Contains(obj),
            _ => throw new NotImplementedException($"Unknown area condition: {condition}")
        };

        public bool Evaluate(Bounds obj, Bounds area) => condition switch
        {
            AreaCondition.Intersects => obj.Intersects(area),
            AreaCondition.Contains => area.Contains(obj),
            _ => throw new NotImplementedException($"Unknown area condition: {condition}")
        };

    }

    extension(BoundsInt b)
    {
        public bool Intersects(BoundsInt b2) 
            => b.xMin < b2.xMax && b.xMax > b2.xMin
            && b.yMin < b2.yMax && b.yMax > b2.yMin
            && b.zMin < b2.zMax && b.zMax > b2.zMin;
        public bool Contains(BoundsInt b2) 
            => b.xMin <= b2.xMin && b.xMax >= b2.xMax
            && b.yMin <= b2.yMin && b.yMax >= b2.yMax
            && b.zMin <= b2.zMin && b.zMax >= b2.zMax;
    }

    extension(Bounds b)
    {
        public bool Contains(Bounds b2)
        {
            var min = b.min;
            var max = b.max;
            var min2 = b2.min;
            var max2 = b2.max;

            return min.x <= min2.x && max.x >= max2.x
                && min.y <= min2.y && max.y >= max2.y
                && min.z <= min2.z && max.z >= max2.z;
        }
    }

}
