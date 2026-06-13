namespace ModdableTimberborn.Helpers;

public static class AreaExtensions
{

    extension(BlockObject bo)
    {
        public BoundsInt GetBounds()
        {
            var coords = bo.Coordinates;
            var size = bo._blockObjectSpec.Size;
            return new(coords, size);
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
