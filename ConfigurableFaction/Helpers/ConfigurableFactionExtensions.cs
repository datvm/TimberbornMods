namespace ConfigurableFaction.Helpers;

public static class ConfigurableFactionExtensions
{

    public static FrozenDictionary<string, T> GetSpecs<T>(this ISpecService specs, Func<T, string> idFunc)
        where T : ComponentSpec
        => specs.GetSpecs<T>().ToFrozenDictionary(idFunc);

    public static ImmutableArray<T> Map<T>(this IEnumerable<string> ids, IDictionary<string, T> dict) => [.. ids.Select(id => dict[id])];

    public static ImmutableArray<T> Map<T>(this IEnumerable<string> ids, IDictionary<string, T> dict, bool skipNonExisting)
    {
        return skipNonExisting 
            ? [.. ids.Where(dict.ContainsKey).Select(id => dict[id])] 
            : ids.Map(dict);
    }

    public static void RemoveNoLongerExistEntries<T>(this HashSet<string> items, IDictionary<string, T> ids)
    {
        var removingIds = items.Where(id => !ids.ContainsKey(id)).ToArray();
        foreach (var id in removingIds)
        {
            items.Remove(id);
        }
    }

    public static bool Match(this SettingsFilter filter, string text, bool isChecked)
    {
        if (isChecked)
        {
            if (!filter.ShowChecked) { return false; }
        } else
        {
            if (!filter.ShowUnchecked) { return false; }
        }

        return text.Length == 0 || text.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase);
    }

}
