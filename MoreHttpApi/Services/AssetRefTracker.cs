namespace MoreHttpApi.Services;

public static class AssetRefTracker
{

    public static readonly ConditionalWeakTable<Object, string> AssetPaths = [];

    public static string? TryGetAssetPath(Object asset) 
        => AssetPaths.TryGetValue(asset, out var path) ? path : null;

}
