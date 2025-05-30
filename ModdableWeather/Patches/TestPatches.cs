namespace ModdableWeather.Patches;

[HarmonyPatch]
public static class TestPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ModSystemFileProvider<Sprite>), nameof(ModSystemFileProvider<>.Load))]
    public static void PrintOut(ModSystemFileProvider<Sprite> __instance)
    {
        Debug.Log(string.Join(Environment.NewLine, __instance._filePaths.Keys));
    }

}
