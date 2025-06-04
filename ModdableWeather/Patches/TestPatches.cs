namespace ModdableWeather.Patches;

#warning Delete this before release
[HarmonyPatch]
public static class TestPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ModAssetBundleProvider), nameof(ModAssetBundleProvider.Load))]
    public static void PrintPaths(ModAssetBundleProvider __instance)
    {
        Debug.Log(string.Join(Environment.NewLine, __instance._assetPaths.Keys));
    }

}
