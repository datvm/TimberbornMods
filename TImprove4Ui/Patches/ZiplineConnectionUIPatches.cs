namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class ZiplineConnectionUIPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ZiplineConnectionButtonFactory), nameof(ZiplineConnectionButtonFactory.SetForConnection))]
    public static void SetButtonName(ZiplineTower otherZiplineTower, Button button)
    {
        var name = StaticSingletonsService.EntityBadgeService?.GetEntityName(otherZiplineTower);
        if (name is null) { return; }

        ZiplineConnectionButtonFactory.SetName(button, name);
    }

}
