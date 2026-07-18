namespace HousePainter.Helpers;

public static class MaterialNames
{

    const string InstanceSuffix = " (Instance)";

    public static string Clean(string name)
    {
        var cleaned = name.EndsWith(InstanceSuffix, StringComparison.Ordinal)
            ? name[..^InstanceSuffix.Length]
            : name;

        var slash = cleaned.LastIndexOf('/');
        return slash >= 0 ? cleaned[(slash + 1)..] : cleaned;
    }

    public static bool IsTrackerNoise(string name) =>
        name.StartsWith("@", StringComparison.Ordinal);

}
