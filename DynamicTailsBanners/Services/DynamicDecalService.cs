namespace DynamicTailsBanners.Services;

[BindSingleton]
public class DynamicDecalService(IEnumerable<IDynamicDecalProvider> providers)
{

    readonly FrozenDictionary<string, IDynamicDecalProvider> providers = providers.ToFrozenDictionary(p => p.Id);

    public IDynamicDecalProvider? GetProvider(string id)
        => providers.TryGetValue(id, out var p) ? p : null;

    public IDynamicTailDecalProvider? GetTail(string id)
        => GetProvider<IDynamicTailDecalProvider>(id);

    public IDynamicBannerDecalProvider? GetBanner(string id)
        => GetProvider<IDynamicBannerDecalProvider>(id);

    public T? GetProvider<T>(string id) where T : IDynamicDecalProvider
    {
        var p = GetProvider(id);
        if (p is null) { return default; }

        return p is T specific 
            ? specific 
            : throw new InvalidOperationException($"Provider {p.GetType().Name} with id '{id}' does not implement {typeof(T).Name}.");
    }
}