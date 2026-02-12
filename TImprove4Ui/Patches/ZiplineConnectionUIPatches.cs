namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class ZiplineConnectionUIPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ZiplineConnectionButtonFactory), nameof(ZiplineConnectionButtonFactory.SetForConnection))]
    public static void SetButtonName(ZiplineTower otherZiplineTower, Button button)
    {
        var name = otherZiplineTower.GetComponent<NamedEntity>()?.EntityName;
        if (string.IsNullOrEmpty(name)) { return; }

        ZiplineConnectionButtonFactory.SetName(button, name);
    }

}
