namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));
        h.PatchAll();
    }


}

[HarmonyPatch]
public static class TestPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(ModAssetBundleProvider), nameof(ModAssetBundleProvider.CachePaths))]
    public static void AfterLoad(ModAssetBundleProvider __instance)
    {
        foreach (var item in __instance._assetPaths.Keys)
        {
            Debug.Log(item);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SurfaceBlockCollectionFactory), nameof(SurfaceBlockCollectionFactory.AddAllVariations))]
    public static void Prefix2(GameObject model)
    {
        var shader = ShaderService.Instance.Shader;
        if (shader is null)
        {
            Debug.LogError("TerrainShader is null!");
            return;
        }

        var renderer = model.GetComponent<MeshRenderer>();
        if (!renderer) { return; }

        var materials = renderer.sharedMaterials.ToArray();
        foreach (var m in materials)
        {
            if (m.shader.name.Contains("TerrainURP"))
            {
                Debug.Log($"Replacing shader on material {m.name} from {m.shader.name} to {shader.name}");
                m.shader = shader;
                m.renderQueue = 3005;
            }
        }
        renderer.sharedMaterials = materials;

        Shader.SetGlobalFloat("_TerrainAlpha", .5f);
    }

}
