namespace MoreOverlay.Patches;

[HarmonyPatch(typeof(StockpileOverlay))]
public static class StockpileOverlayPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(StockpileOverlay.Enable))]
    public static void OnEnabled() => MoreOverlayService.Instance?.SetOverlayActive(true);

    [HarmonyPostfix, HarmonyPatch(nameof(StockpileOverlay.Disable))]
    public static void OnDisabled() => MoreOverlayService.Instance?.SetOverlayActive(false);

}
