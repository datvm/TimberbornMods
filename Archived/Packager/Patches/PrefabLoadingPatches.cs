namespace Packager.Patches;

[HarmonyPatch(typeof(AssetLoader))]
public static class PrefabLoadingPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(AssetLoader.Load), [typeof(string), typeof(Type)])]
    public static void OnLumberjackLoaded(string path, object __result)
    {
        if (__result is not GameObject obj
            || !path.Contains(PackagerPrefabProvider.ExpectedPrefabName, StringComparison.OrdinalIgnoreCase)) { return; }

        PackagerPrefabProvider.LumberjackPrefab = new(obj);
    }

}
