namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class ThumbnailPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SteamWorkshopModThumbnail), nameof(SteamWorkshopModThumbnail.TryGetCustomThumbnailPath))]
    public static bool PatchThumbnailPath(ref bool __result, out string? previewPath)
    {
        previewPath = null;
        if (!MSettings.PickThumbnail) { return true; }

        previewPath = MSettings.SystemFileDialogService?.ShowOpenFileDialog(".png;.jpg;.jpeg");
        __result = previewPath is not null;

        return false;
    }

}
