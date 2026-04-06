namespace TImprove4Modders.Patches;

public static class ThumbnailPatches
{

    public static bool PatchThumbnailPath(ref bool __result, out string? previewPath)
    {
        previewPath = null;
        if (!MSettings.PickThumbnail) { return true; }

        previewPath = MSettings.SystemFileDialogService?.ShowOpenFileDialog(".png;.jpg;.jpeg");
        __result = previewPath is not null;

        return false;
    }

}
