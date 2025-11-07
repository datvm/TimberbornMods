namespace TImprove4Modders.Patches;

[HarmonyPatch(typeof(SaveModsValidator))]
public static class SaveModsValidatorPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(SaveModsValidator.ShowModsIncompatibilityDialog))]
    public static void AfterDialogShowed(SaveModsValidator __instance)
    {
        if (!MSettings.AutoSkipLoadModCompat || SaveModsDialogService.Instance is null)
        {
            return;
        }

        var panel = __instance._dialogBoxShower._panelStack.TopPanel;
        SaveModsDialogService.Instance.ActivateFor(panel);
    }

}
