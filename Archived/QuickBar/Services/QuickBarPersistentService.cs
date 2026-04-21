namespace QuickBar.Services;

public class QuickBarPersistentService(
    IEnumerable<IQuickBarItemProvider> providers
) : ILoadableSingleton
{

    public FrozenDictionary<Type, IQuickBarItemProvider> SerializersByTypes { get; private set; } = FrozenDictionary<Type, IQuickBarItemProvider>.Empty;
    public FrozenDictionary<string, IQuickBarItemProvider> SerializerTypesByNames { get; private set; } = FrozenDictionary<string, IQuickBarItemProvider>.Empty;

    public void Load()
    {
        LoadDict();
    }

    void LoadDict()
    {
        Dictionary<Type, IQuickBarItemProvider> dict = [];

        foreach (var s in providers)
        {
            foreach (var t in s.SupportedType)
            {
                if (dict.ContainsKey(t))
                {
                    throw new InvalidOperationException($"Serializer for {t} is already handled by {dict[t].GetType().FullName}.");
                }

                dict[t] = s;
            }
        }

        SerializersByTypes = dict.ToFrozenDictionary();
        SerializerTypesByNames = providers.ToFrozenDictionary(q => q.GetType().FullName);
    }

    public string Serialize(IQuickBarItem? item)
    {
        if (item == null) { return ""; }

        if (!SerializersByTypes.TryGetValue(item.GetType(), out var s))
        {
            Debug.LogWarning($"No serializer found for {item.GetType().FullName}. Ignoring.");
            return "";
        }

        return $"{s.GetType().FullName}|{s.Serialize(item)}";
    }

    public IQuickBarItem? Deserialize(string? data)
    {
        if (string.IsNullOrEmpty(data)) { return null; }

        var parts = data.Split('|', 2);

        if (parts.Length < 2 || parts[1].Length == 0)
        {
            Debug.LogWarning($"Invalid data format: {data}. Ignoring.");
            return null;
        }

        if (!SerializerTypesByNames.TryGetValue(parts[0], out var s))
        {
            Debug.LogWarning($"No serializer found for {parts[0]}. Ignoring.");
            return null;
        }

        return s.Deserialize(parts[1]);
    }


}
