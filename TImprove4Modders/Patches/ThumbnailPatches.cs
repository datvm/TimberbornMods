using System.Windows.Forms;
using Timberborn.SteamWorkshopModUploadingUI;

namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class ThumbnailPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SteamWorkshopModThumbnail), nameof(SteamWorkshopModThumbnail.TryGetCustomThumbnailPath))]
    public static bool PatchThumbnailPath(ref bool __result, out string? previewPath)
    {
        previewPath = null;
        if (!MSettings.PickThumbnail) { return true; }

        OpenFileDialog diag = new()
        {
            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        };

        if (diag.ShowDialog() == DialogResult.OK)
        {
            previewPath = diag.FileName;
            __result = true;
        }
        else
        {
            __result = false;
        }

        return false;
    }

}
