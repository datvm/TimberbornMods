namespace DynamicTailsBanners.Patches;

[HarmonyPatch(typeof(DecalSupplierBuildingIcon))]
public static class DecalSupplierBuildingIconPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(DecalSupplierBuildingIcon.UpdateIcon))]
    public static bool InterceptTexture(DecalSupplierBuildingIcon __instance)
        => !__instance.GetComponent<DynamicDecalSupplierBuildingIcon>().RevalidateAndShowTexture();

}
