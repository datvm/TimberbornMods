namespace TailsAndBannersModMaker.Services;

[BindMenuSingleton]
public class DecalProvider(UserDecalService userDecalService)
{
    readonly Dictionary<string, ImmutableArray<DecalSpec>> cache = [];

    public ImmutableArray<DecalSpec> GetDecals(string type)
    {
        if (!cache.TryGetValue(type, out var decals))
        {
            decals = cache[type] = [.. userDecalService.GetCustomDecals(type)];
        }

        return decals;
    }

}
