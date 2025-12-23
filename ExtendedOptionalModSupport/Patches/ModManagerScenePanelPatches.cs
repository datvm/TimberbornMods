
namespace ExtendedOptionalModSupport.Patches;

[HarmonyPatch(typeof(ModManagerScenePanel))]
public static class ModManagerScenePanelPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(ModManagerScenePanel.StartGame))]
    public static void LoadOptionalAssemblies(ModManagerScenePanel __instance)
    {
        var repo = __instance.CreateModRepository();
        new OptionalModStarter(repo).StartOptionalMods();
    }

}
