namespace HousePainter.Services;

/// <summary>
/// Records original material names while prefabs are auto-atlased (see AutoAtlasingPatches).
/// Keyed by prefab usage name from the optimizer.
/// </summary>
public static class PrefabMaterialTracker
{

    static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> materialsByUsage =
        new(StringComparer.Ordinal);

    public static void Record(string usageName, string? materialName)
    {
        if (string.IsNullOrEmpty(usageName) || string.IsNullOrEmpty(materialName))
        {
            return;
        }

        if (MaterialNames.IsTrackerNoise(materialName))
        {
            return;
        }

        var set = materialsByUsage.GetOrAdd(usageName, static _ => new(StringComparer.Ordinal));
        set.TryAdd(MaterialNames.Clean(materialName), 0);
    }

    public static void RecordMany(string usageName, IEnumerable<string?> materialNames)
    {
        foreach (var name in materialNames)
        {
            Record(usageName, name);
        }
    }

    public static ImmutableArray<string> GetForUsage(string usageName)
    {
        if (!materialsByUsage.TryGetValue(usageName, out var set))
        {
            return [];
        }

        return [.. set.Keys.OrderBy(static n => n, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Prefab usage names and template names are not always identical.
    /// </summary>
    public static ImmutableArray<string> GetForTemplate(string templateName)
    {
        var exact = GetForUsage(templateName);
        if (exact.Length > 0)
        {
            return exact;
        }

        foreach (var (usage, set) in materialsByUsage)
        {
            if (usage.Equals(templateName, StringComparison.OrdinalIgnoreCase)
                || usage.Contains(templateName, StringComparison.OrdinalIgnoreCase)
                || templateName.Contains(usage, StringComparison.OrdinalIgnoreCase))
            {
                return [.. set.Keys.OrderBy(static n => n, StringComparer.Ordinal)];
            }
        }

        return [];
    }

    public static IReadOnlyCollection<string> KnownUsages => materialsByUsage.Keys.ToArray();

    public static void Clear() => materialsByUsage.Clear();

}
