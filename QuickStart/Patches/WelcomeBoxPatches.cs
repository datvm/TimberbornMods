global using Timberborn.MainMenuScene;

namespace QuickStart.Patches;

[HarmonyPatch]
public static class WelcomeBoxPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MainMenuInitializer), nameof(MainMenuInitializer.ShowWelcomeScreen))]
    public static bool OverrideWelcomeScreen(MainMenuInitializer __instance)
    {
        __instance.ShowMainMenuPanel();

        return false;
    }

}
