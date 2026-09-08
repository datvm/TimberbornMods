namespace MapResizer;

public static class MapResizeExtensions
{

    public static int GetLength(this Vector3Int size) => size.x * size.y * size.z;

}
