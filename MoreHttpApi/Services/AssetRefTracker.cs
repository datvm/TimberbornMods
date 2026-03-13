namespace MoreHttpApi.Services;

public static class AssetRefTracker
{

    public static readonly ConditionalWeakTable<UnityEngine.Object, string> AssetPaths = [];

    public static string? TryGetAssetPath(UnityEngine.Object asset) 
        => AssetPaths.TryGetValue(asset, out var path) ? path : null;

}
