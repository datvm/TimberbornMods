namespace DisableHauling.Patches;

[HarmonyPatch(typeof(HaulCandidateFragment))]
public static class HaulCandidateFragmentPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(HaulCandidateFragment.InitializeFragment))]
    public static void Initialize(HaulCandidateFragment __instance)
    {
        DisableHaulingFragment.fragment = __instance;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(HaulCandidateFragment.ShowFragment))]
    public static void ShowFragment(BaseComponent entity)
    {
        DisableHaulingFragment.instance?.ShowFragment(entity);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(HaulCandidateFragment.ClearFragment))]
    public static void ClearFragment()
    {
        DisableHaulingFragment.instance?.ClearFragment();
    }

}
