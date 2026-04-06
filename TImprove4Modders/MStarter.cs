namespace TImprove4Modders;

public class MStarter : IModStarter
{
    public static string ModPath { get; private set; } = null!;

    public void StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;

        var h = new Harmony(nameof(TImprove4Modders));
        h.PatchAll();

        // Patch Steam
        var loadedAssemblies = TimberUiUtils.LoadedAssemblyNames;
        if (loadedAssemblies.Contains("Timberborn.SteamOverlaySystem") &&
            loadedAssemblies.Contains("Timberborn.SteamWorkshopModUploadingUI"))
        {
            PatchSteam(h);
        }
    }

    static void PatchSteam(Harmony h)
    {
        h.Patch(
            typeof(SteamOverlayOpener).Method(nameof(SteamOverlayOpener.OpenSteamPage)),
            prefix: typeof(SteamWebPatch).Method(nameof(SteamWebPatch.PatchSteamOverlayBrowser))
        );

        h.Patch(
            typeof(SteamWorkshopModThumbnail).Method(nameof(SteamWorkshopModThumbnail.TryGetCustomThumbnailPath)),
            prefix: typeof(ThumbnailPatches).Method(nameof(ThumbnailPatches.PatchThumbnailPath))
        );
    }

}