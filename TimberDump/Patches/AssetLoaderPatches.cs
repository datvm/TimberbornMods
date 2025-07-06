namespace TimberDump.Patches;

[HarmonyPatch]
public static class AssetLoaderPatches
{

    public static readonly ConditionalWeakTable<UnityEngine.Object, string> AssetPaths = [];

    [HarmonyPostfix, HarmonyPatch(typeof(AssetLoader), nameof(AssetLoader.Load), [typeof(string), typeof(Type)])]
    public static void OnAssetLoaded(string path, UnityEngine.Object __result)
    {
        if (!__result) { return; }

        AssetPaths.AddOrUpdate(__result, path);
    }

}
