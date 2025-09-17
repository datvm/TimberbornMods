namespace UnityEngine;

public static partial class TimberUIHelperExtensions
{

    public static Color ToColor255(this Vector3Int v) => new(v.x / 255f, v.y / 255f, v.z / 255f);
    public static Color ToColor255(this Vector3 v) => new(v.x / 255f, v.y / 255f, v.z / 255f);
    public static Vector3 ToColor255(this Color c) => new(c.r * 255f, c.g * 255f, c.b * 255f);
    public static Vector3Int ToColor255Int(this Color c) => new(Mathf.RoundToInt(c.r * 255f), Mathf.RoundToInt(c.g * 255f), Mathf.RoundToInt(c.b * 255f));

    public static Color ToColor(this Vector3 v) => new(v.x, v.y, v.z);
    public static Color ToColor(this Vector4 v) => new(v.x, v.y, v.z, v.w);
    public static Vector3 ToVector3(this Color c) => new(c.r, c.g, c.b);
    public static Vector4 ToVector4(this Color c) => new(c.r, c.g, c.b, c.a);

}
