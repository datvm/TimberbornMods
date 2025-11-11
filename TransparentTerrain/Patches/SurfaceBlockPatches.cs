namespace TransparentTerrain.Patches;

[HarmonyPatch]
public static class SurfaceBlockPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SurfaceBlockCollectionFactory), nameof(SurfaceBlockCollectionFactory.AddAllVariations))]
    public static void ChangeBlockShaders(GameObject model)
    {
        var instance = TransparentTerrainService.Instance
            ?? throw new InvalidOperationException("TransparentTerrainService instance is null in SurfaceBlockPatches.ChangeBlockShaders");

        var renderers = model.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            foreach (var m in r.sharedMaterials)
            {
                instance.RegisterMaterial(m);
            }
        }
    }

}
