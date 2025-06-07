namespace ConfigurableFaction.Services;

public class FactionOptionsProvider(
    FactionSpecService factionSpecService,
    PersistentService persistentService
) : ILoadableSingleton, IUnloadableSingleton
{
    public static FrozenDictionary<string, FactionOptions> LatestFactionOptions = FrozenDictionary<string, FactionOptions>.Empty;
    public FrozenDictionary<string, FactionOptions> FactionOptions { get; private set; } = null!;

    public void Load()
    {
        LoadFactions();
    }

    void LoadFactions()
    {
        var factions = factionSpecService.Factions;

        FactionOptions = factions
            .Select(q => (q.Id, persistentService.Load<FactionOptions>(q.Id) ?? new(q.Id)))
            .ToFrozenDictionary(q => q.Id, q => q.Item2);
    }

    public void Save(string factionId) => persistentService.Save(factionId, FactionOptions[factionId]);

    public void Unload()
    {
        LatestFactionOptions = FactionOptions;
    }

    public void Reset()
    {
        persistentService.Clear();
        LoadFactions();
    }

}
