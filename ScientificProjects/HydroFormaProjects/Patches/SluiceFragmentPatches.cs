namespace HydroFormaProjects.Patches;

[HarmonyPatch(typeof(SluiceFragment))]
public static class SluiceFragmentPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(SluiceFragment.InitializeFragment))]
    public static void OnInitializeFragment(SluiceFragment __instance)
    {
        if (SluiceUpstreamFragment.Instance is null)
        {
            SluiceUpstreamFragment.PendingFragment = __instance;
        }
        else
        {
            SluiceUpstreamFragment.Instance.InitializeFragment(__instance);
        }
    }

    [HarmonyPostfix, HarmonyPatch(nameof(SluiceFragment.ShowFragment))]
    public static void OnShowFragment() => SluiceUpstreamFragment.Instance?.ShowFragment();

    [HarmonyPostfix, HarmonyPatch(nameof(SluiceFragment.ClearFragment))]
    public static void OnClearFragment() => SluiceUpstreamFragment.Instance?.ClearFragment();

}
