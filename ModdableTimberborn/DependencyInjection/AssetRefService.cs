namespace ModdableTimberborn.DependencyInjection;

public class AssetRefService(IAssetLoader assets)
{

    public AssetRef<T> CreateAssetRef<T>(string path)
        where T : Object 
        => new(path, new(() => assets.Load<T>(path)));

    public IEnumerable<AssetRef<T>> CreateAssetRefs<T>(IEnumerable<string> paths)
        where T : Object
        => paths.Select(CreateAssetRef<T>);

    public AssetRef<BlueprintAsset> CreateBlueprintAssetRef(string path)
        => CreateAssetRef<BlueprintAsset>(path);

    public IEnumerable<AssetRef<BlueprintAsset>> CreateBlueprintAssetRefs(IEnumerable<string> paths)
        => CreateAssetRefs<BlueprintAsset>(paths);

}

