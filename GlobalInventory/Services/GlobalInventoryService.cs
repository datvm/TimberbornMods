namespace GlobalInventory.Services;

[BindSingleton]
public class GlobalInventoryService(GlobalGoodSpecService specs, ISingletonLoader loader, EventBus eb)
    : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(GlobalInventory));
    static readonly ListKey<GlobalGoodHandle> GoodsKey = new("Goods");

    FrozenDictionary<string, GlobalGoodHandle> byId = FrozenDictionary<string, GlobalGoodHandle>.Empty;

    public IReadOnlyList<GlobalGoodHandle> All { get; private set; } = [];
    public IEnumerable<GlobalGoodHandle> Visible => All.Where(q => q.IsVisible);

    public void Load()
    {
        Dictionary<string, GlobalGoodHandle> map = [];
        foreach (var spec in specs.All)
        {
            map[spec.Id] = new(spec, eb);
        }

        if (loader.TryGetSingleton(SaveKey, out var s) && s.Has(GoodsKey))
        {
            foreach (var saved in s.Get(GoodsKey, GetSerializer()))
            {
                map[saved.Spec.Id] = saved;
            }
        }

        byId = map.ToFrozenDictionary();
        All = [.. specs.All.Select(q => map[q.Id])];
    }

    public GlobalGoodHandle Get(string id)
    {
        if (TryGet(id, out var handle))
        {
            return handle;
        }

        throw new ArgumentException($"Global good '{id}' is not defined.");
    }

    public bool TryGet(string id, [NotNullWhen(true)] out GlobalGoodHandle? handle) => byId.TryGetValue(id, out handle);

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(SaveKey);
        s.Set(GoodsKey, All, GetSerializer());
    }

    GlobalGoodHandle.Serializer GetSerializer() => new(specs, eb);
}
