namespace HousePainter.Patches;

/// <summary>
/// Captures original material names before AutoAtlasing merges them into FactionAtlas.
/// </summary>
[HarmonyPatch]
public static class AutoAtlasingPatches
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AutoAtlasingPrefabOptimizer), "OptimizeMeshRenderer")]
    public static void OptimizeMeshRendererPrefix(MeshRenderer meshRenderer, ref string?[]? __state)
    {
        if (!meshRenderer)
        {
            __state = null;
            return;
        }

        var shared = meshRenderer.sharedMaterials;
        var names = new string?[shared.Length];
        for (var i = 0; i < shared.Length; i++)
        {
            names[i] = shared[i] ? shared[i].name : null;
        }

        __state = names;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AutoAtlasingPrefabOptimizer), "OptimizeMeshRenderer")]
    public static void OptimizeMeshRendererPostfix(string usageName, string?[]? __state)
    {
        if (__state is null || __state.Length == 0)
        {
            return;
        }

        PrefabMaterialTracker.RecordMany(usageName, __state);
    }

}
