namespace MapResizer.Helpers;

public class FlatArrayHelper<T>(T[] array, Vector3Int size)
{
    public static readonly Vector3Int PaddingSize = new(1, 1, 0);

    public T[] Array => array;
    public Vector3Int Size => size;
    public int Length => array.Length;

    public int Stride { get; } = size.x;
    public int VerticalStride { get; } = size.x * size.y;

    public T this[int x, int y, int z]
    {
        get => array[GetIndex(x, y, z)];
        set => array[GetIndex(x, y, z)] = value;
    }

    public int GetIndex(int x, int y, int z)
    {
        return x + y * Stride + z * VerticalStride;
    }

    public FlatArrayHelper(Vector3Int size) : this(new T[size.GetLength()], size) { }

    public FlatArrayHelper<T> PrintDebug(string filePath)
    {
        File.WriteAllText(
            filePath, 
            $"{size.x};{size.y};{size.z};" +
            string.Join("", array.Select(q => q.ToString() == "True" ? "1" : "0")));

        return this;
    }

    public FlatArrayHelper<T> AddPadding() => Pad(PaddingSize);
    public FlatArrayHelper<T> RemovePadding() => Pad(-PaddingSize);

    public FlatArrayHelper<T> Pad(Vector3Int paddingSize) // paddingSize can be positive or negative (for removing padding)
    {
        var newSize = Size + paddingSize * 2;
        var arr = new T[newSize.GetLength()];
        var result = new FlatArrayHelper<T>(arr, newSize);

        var dx = paddingSize.x;
        var dy = paddingSize.y;
        var dz = paddingSize.z;

        for (int x = 0; x < newSize.x; x++)
        {
            for (int y = 0; y < newSize.y; y++)
            {
                for (int z = 0; z < newSize.z; z++)
                {
                    var sourceX = x - dx;
                    var sourceY = y - dy;
                    var sourceZ = z - dz;

                    // Ensure the source indices are within bounds of the original array
                    if (sourceX >= 0 && sourceX < Size.x &&
                        sourceY >= 0 && sourceY < Size.y &&
                        sourceZ >= 0 && sourceZ < Size.z)
                    {
                        result[x, y, z] = this[sourceX, sourceY, sourceZ];
                    }
                }
            }
        }

        return result;
    }

}
