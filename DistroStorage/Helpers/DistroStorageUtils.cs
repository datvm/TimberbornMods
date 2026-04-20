namespace DistroStorage.Helpers;

public static class DistroStorageUtils
{
    static readonly Vector3Int Max = new(int.MaxValue, int.MaxValue, int.MaxValue);
    static readonly Vector3Int Min = new(int.MinValue, int.MinValue, int.MinValue);

    extension(BlockObject bo)
    {

        public BoundsInt GetBounds()
        {
            var max = Min; var min = Max;

            foreach (var block in bo.PositionedBlocks.GetAllBlocks())
            {
                var coord = block.Coordinates;

                for (int i = 0; i < 3; i++)
                {
                    var e = coord[i];
                    if (e > max[i]) { max[i] = e; }
                    if (e < min[i]) { min[i] = e; }
                }
            }

            return new(min, max - min + Vector3Int.one);
        }

    }

}
