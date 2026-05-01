namespace DynamicTailsBanners.Patches;

[HarmonyPatch(typeof(DecalSupplierFragment))]
public static class DecalSupplierFragmentPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(DecalSupplierFragment.ShowFragment))]
    public static void OnShowFragment() => DynamicDecalFragment.Instance?.ShowFragment();

    [HarmonyPostfix, HarmonyPatch(nameof(DecalSupplierFragment.UpdateFragment))]
    public static void OnUpdateFragment() => DynamicDecalFragment.Instance?.UpdateFragment();

    [HarmonyPostfix, HarmonyPatch(nameof(DecalSupplierFragment.ClearFragment))]
    public static void OnClearFragment() => DynamicDecalFragment.Instance?.ClearFragment();

}
