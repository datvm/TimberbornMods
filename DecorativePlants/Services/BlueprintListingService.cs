namespace DecorativePlants.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class BlueprintListingService(
    ISpecService specs
)
{
    const string CommonId = "Common";
    bool initialized;

    public FrozenDictionary<string, ImmutableArray<string>> ItemsOfCollection { get; private set; } = FrozenDictionary<string, ImmutableArray<string>>.Empty;
    public FrozenDictionary<string, ImmutableArray<string>> FactionCollections { get; private set; } = FrozenDictionary<string, ImmutableArray<string>>.Empty;

    public void Initialize()
    {
        if (initialized) { return; }
        initialized = true;

        Dictionary<string, List<string>> itemsOfCollection = [];

        foreach (var col in specs.GetSpecs<TemplateCollectionSpec>())
        {
            var list = itemsOfCollection.GetOrAdd(col.CollectionId, () => []);
            list.AddRange(col.Blueprints.Select(bp => bp.Path));
        }

        ItemsOfCollection = itemsOfCollection.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray());

        FactionCollections = specs.GetSpecs<FactionSpec>()
            .Append(new() { Id = CommonId, TemplateCollectionIds = [CommonId] })
            .ToFrozenDictionary(f => f.Id, f => f.TemplateCollectionIds);
    }

    public IEnumerable<string> GetFactionBlueprints(string factionId)
    {
        var collectionIds = FactionCollections[factionId].Prepend(CommonId);
        foreach (var collectionId in collectionIds)
        {
            var items = ItemsOfCollection[collectionId];

            foreach (var item in items)
            {
                yield return item;
            }
        }
    }

    public IEnumerable<string> GetAllFactionBlueprints()
    {
        HashSet<string> uniques = [];

        foreach (var fId in FactionCollections.Keys)
        {
            foreach (var path in GetFactionBlueprints(fId))
            {
                if (uniques.Add(path))
                {
                    yield return path;
                }
            }
        }
    }

}
