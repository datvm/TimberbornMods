global using Timberborn.MainMenuPanels;

namespace QuickStart.Patches;

[HarmonyPatch]
public static class QuickLoadPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Show))]
    public static void AfterMenuShowed(MainMenuPanel __instance)
    {
        MSettings.MainMenuPanel = __instance;
        MSettings.CheckAutoLoading();

    }

}
