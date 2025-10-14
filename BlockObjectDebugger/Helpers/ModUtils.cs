namespace BlockObjectDebugger.Helpers;

public static class ModUtils
{
    public static readonly ImmutableArray<BlockOccupations> AllOccupations = [.. Enum.GetValues(typeof(BlockOccupations))
        .Cast<BlockOccupations>()
        .OrderBy(q => q)];

    public static readonly ImmutableArray<string> OccupationStrings = [..AllOccupations
        .Select(e => e.ToString())];

    public const string BuildingsPath = $"Buildings/{nameof(BlockObjectDebugger)}/";
    public const string PrefabPrefix = $"{nameof(BlockObjectDebugger)}.";

    public static readonly ImmutableArray<Color> Colors = [
        new(0, 0, 0), // Black (not used)
        new(0.63f, 0.78f, 0.95f), // Light blue
        new(1.00f, 0.71f, 0.51f), // Light orange
        new(0.55f, 0.90f, 0.63f), // Mint green
        new(1.00f, 0.62f, 0.61f), // Salmon
        new(0.82f, 0.73f, 1.00f), // Lavender
        new(0.87f, 0.73f, 0.61f), // Beige
        new(0.98f, 0.69f, 0.89f), // Rose
        new(0.81f, 0.81f, 0.81f), // Gray
        new(0.73f, 0.95f, 0.94f), // Aqua tint
        new(0.71f, 0.59f, 0.91f), // Periwinkle
    ];

    public static string GetPrefabName(string occupation) => $"{PrefabPrefix}{occupation}";
    public static string GetBuildingPath(string occupation) => $"{BuildingsPath}{GetPrefabName(occupation)}";

    public static bool IsBuildingPath(string path, [NotNullWhen(true)] out string? occupation)
    {
        occupation = null;
        if (!path.StartsWith(BuildingsPath, StringComparison.OrdinalIgnoreCase)) { return false; }

        var prefabName = path[BuildingsPath.Length..];
        occupation = prefabName[PrefabPrefix.Length..];

        foreach (var o in OccupationStrings)
        {
            if (string.Equals(o, occupation, StringComparison.OrdinalIgnoreCase))
            {
                occupation = o;
                return true;
            }
        }

        return false;
    }

    public static void AppendLoc(string key, string value)
    {
        var loc = (Loc)ContainerRetriever.GetInstance<ILoc>();
        loc._localization[key] = value;
    }

    public static string ToCoordsString(this Vector3 v) => $"({v.x:0.##}, {v.y:0.##}, {v.z:0.##})";
    public static string ToCoordsString(this Vector3Int v) => $"({v.x}, {v.y}, {v.z})";

}
