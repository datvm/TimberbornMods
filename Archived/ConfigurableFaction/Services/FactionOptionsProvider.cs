namespace ConfigurableFaction.Services;

public class FactionOptionsProvider(
    PersistentService persistentService
) : ILoadableSingleton, IUnloadableSingleton
{
    public const string PrefabGroupPrefix = "ConfigurableFaction.";
    public static FrozenDictionary<string, FactionOptions> LatestFactionOptions = FrozenDictionary<string, FactionOptions>.Empty;
    public FrozenDictionary<string, FactionOptions> FactionOptions { get; private set; } = null!;

    public void Load()
    {
        LoadFactions();
    }

    void LoadFactions()
    {
        var savedFactionIds = persistentService.GetSavedFactionIds();

        FactionOptions = savedFactionIds
            .Select(id => (id, persistentService.Load<FactionOptions>(id) ?? new(id)))
            .ToFrozenDictionary(q => q.id, q => q.Item2);
    }

    public void AddMissingFactions(IEnumerable<string> allIds)
    {
        FactionOptions = FactionOptions.Concat(allIds
            .Where(q => !FactionOptions.ContainsKey(q))
            .Select(q => new KeyValuePair<string, FactionOptions>(q, new(q))))
            .ToFrozenDictionary();
    }

    public void Save(string factionId) => persistentService.Save(factionId, FactionOptions[factionId]);

    public void Unload()
    {
        LatestFactionOptions = FactionOptions;
    }

    public void Reset()
    {
        var keys = FactionOptions.Keys.ToImmutableArray();

        persistentService.Clear();
        LoadFactions();

        AddMissingFactions(keys);
    }

    public static string GetPrefabGroupId(string factionId) => PrefabGroupPrefix + factionId;

}
