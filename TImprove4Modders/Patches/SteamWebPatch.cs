namespace TImprove4Modders.Patches;

public static class SteamWebPatch
{

    public static bool PatchSteamOverlayBrowser(string page)
    {
        if (!MSettings.OpenExternalBrowser) { return true; }

        Application.OpenURL(page);
        return false;
    }

}
