namespace DynamicTailsBanners.Services;

[BindSingleton]
public class DynamicDecalService(IEnumerable<IDynamicDecalProvider> providers)
{

    readonly FrozenDictionary<string, IDynamicDecalProvider> providers = providers.ToFrozenDictionary(p => p.Id);

    public IDynamicDecalProvider? GetProvider(string id)
        => providers.TryGetValue(id, out var p) ? p : null;

    public IDynamicTailDecalProvider? GetTail(string id)
        => GetSpecific<IDynamicTailDecalProvider>(id);

    T? GetSpecific<T>(string id) where T : IDynamicDecalProvider
    {
        var p = GetProvider(id);
        if (p is null) { return default; }

        return p is T specific 
            ? specific 
            : throw new InvalidOperationException($"Provider with id '{id}' is not of type {typeof(T).Name}.");
    }
}