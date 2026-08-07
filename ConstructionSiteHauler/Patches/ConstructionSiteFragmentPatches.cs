namespace ConstructionSiteHauler.Patches;

[HarmonyPatch(typeof(ConstructionSiteFragment))]
public static class ConstructionSiteFragmentPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(ConstructionSiteFragment.InitializeFragment))]
    public static void Init(ConstructionSiteFragment __instance) => ConstructionSiteHaulingFragment.Instance.InitializeFragment(__instance);

    [HarmonyPostfix, HarmonyPatch(nameof(ConstructionSiteFragment.ShowFragment))]
    public static void Show() => ConstructionSiteHaulingFragment.Instance.ShowFragment();

}
