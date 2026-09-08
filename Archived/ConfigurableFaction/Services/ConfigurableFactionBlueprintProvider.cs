namespace ConfigurableFaction.Services;

public class ConfigurableFactionBlueprintProvider(
    FactionOptionsProvider options
) : IAssetProvider, ILoadableSingleton
{
    public static readonly string BlueprintsPath = SpecService.BlueprintsPath + "/ConfigurableFactions/";
    public bool IsBuiltIn { get; } = false;

    FrozenDictionary<string, OrderedAsset> assets = FrozenDictionary<string, OrderedAsset>.Empty;

    public void Load()
    {
        ReloadFiles();
    }

    public void ReloadFiles()
    {
        Dictionary<string, OrderedAsset> assets = [];

        var counter = 0;
        foreach (var (id, facOptions) in options.FactionOptions)
        {
            var prefabGroupId = FactionOptionsProvider.PrefabGroupPrefix + id;
            var assetName = $"PrefabGroup.{prefabGroupId}.json";
            var assetPath = $"{BlueprintsPath}{assetName}";

            var paths = facOptions.Buildings.Concat(facOptions.Plantables).Concat(facOptions.SpecialBuildings);
            var spec = new PrefabGroupSpec
            {
                Id = FactionOptionsProvider.PrefabGroupPrefix + id,
                Paths = [.. paths],
            };

            var json = JsonConvert.SerializeObject(new
            {
                PrefabGroupSpec = spec
            });

            var textAsset = new TextAsset(json)
            {
                name = assetName,
            };
            assets[assetPath] = new(counter++, textAsset);
        }

        this.assets = assets.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : UnityEngine.Object
    {
        if (typeof(T) != typeof(TextAsset)
            || !path.StartsWith(SpecService.BlueprintsPath, StringComparison.OrdinalIgnoreCase)) { return []; }

        var result = assets
            .Where(q => q.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase))
            .Select(q => q.Value);

        return result;
    }

    public void Reset()
    {
        ReloadFiles();
    }

    public bool TryLoad(string path, Type type, out OrderedAsset orderedAsset)
    {
        orderedAsset = default;

        return type == typeof(TextAsset) && assets.TryGetValue(path, out orderedAsset);
    }
}
