namespace ModdableTimberborn.OptionalMods;

public class OptionalModStarter : IModStarter
{
    public const string OptionalModCategory = $"{nameof(ModdableTimberborn)}.{nameof(OptionalMods)}";

    public void StartMod(IModEnvironment modEnvironment) => new Harmony(OptionalModCategory).PatchCategory(OptionalModCategory);
}

[HarmonyPatch, HarmonyPatchCategory(OptionalModStarter.OptionalModCategory)]
public static class OptionalModPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(ModAssetBundleLoader), nameof(ModAssetBundleLoader.Load))]
    public static void LoadOptionalMods(ModAssetBundleLoader __instance)
    {
        var stackTrace = new System.Diagnostics.StackTrace();
        
        // Make sure to only call it from ModManager
        var hasModManagerInStack = stackTrace.GetFrames()
            .Any(f => f.GetMethod().DeclaringType == typeof(ModManagerScenePanel));

        if (hasModManagerInStack)
        {
            OptionalModsLoader.Load(__instance._modRepository);
        }
    }

}
