namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class BatchControlBoxPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BatchControlBox), nameof(BatchControlBox.OpenTab))]
    public static void PositionBoxBeforeOpen()
    {
        BatchControlBoxService.Instance?.UpdatePanelLocation();
    }

}
