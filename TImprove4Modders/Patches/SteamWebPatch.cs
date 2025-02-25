using Timberborn.SteamOverlaySystem;

namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class SteamWebPatch
{

    [HarmonyPrefix, HarmonyPatch(typeof(SteamOverlayOpener), nameof(SteamOverlayOpener.OpenSteamPage))]
    public static bool PatchSteamOverlayBrowser(string page)
    {
        if (!MSettings.OpenExternalBrowser) { return true; }

        Application.OpenURL(page);
        return false;
    }

}
