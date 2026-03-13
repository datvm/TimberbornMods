namespace MoreHttpApi.Patches;

[HarmonyPatch(typeof(AssetRef<UnityEngine.Object>))]
public static class AssetRefPatches
{

    [HarmonyPostfix, HarmonyPatch(MethodType.Constructor), HarmonyPatch([typeof(string), typeof(Lazy<UnityEngine.Object>)])]
    public static void OnAssetRefCreated(string path, ref Lazy<UnityEngine.Object> ____lazyAsset)
    {
        var lazyAsset = ____lazyAsset;
        ____lazyAsset = new(() => {
            var value = lazyAsset.Value;

            var paths = AssetRefTracker.AssetPaths;
            paths.AddOrUpdate(value, path);

            return value;
        });
    }


}
