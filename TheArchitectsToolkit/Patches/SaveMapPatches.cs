global using Timberborn.MapEditorPersistenceUI;

namespace TheArchitectsToolkit.Patches;

[HarmonyPatch]
public static class SaveMapPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MapPersistenceController), "ForceSaveAs")]
    public static void BeforeSave()
    {
        if (MSettings.LockOnSaveMap)
        {
            ToolkitGameService.Instance?.LockAllUnlockedBuildings();
        }
    }

}
