namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class AutoSavePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Autosaver), nameof(Autosaver.CreateExitSave))]
    public static bool PatchExitSave()
    {
        return !MSettings.NoExitSave;
    }

}
