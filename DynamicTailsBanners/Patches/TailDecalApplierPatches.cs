namespace DynamicTailsBanners.Patches;

[HarmonyPatch(typeof(TailDecalApplier))]
public static class TailDecalApplierPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(TailDecalApplier.ShowTexture))]
    public static bool InterceptTexture(TailDecalApplier __instance) 
        => !__instance.GetComponent<DynamicTailDecalApplier>().ShowTexture();

    [HarmonyPrefix, HarmonyPatch(nameof(TailDecalApplier.ApplyDecal))]
    public static void OnApplyDecal(TailDecalApplier __instance, Decal decal) 
        => __instance.GetComponent<DynamicTailDecalApplier>().OnDecalApplied(decal);

}
