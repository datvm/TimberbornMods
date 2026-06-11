namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));
        h.PatchAll();
    }


}

[HarmonyPatch]
public static class TestPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(ModAssetBundleProvider), nameof(ModAssetBundleProvider.Load))]
    public static void OnLoad(ModAssetBundleProvider __instance)
    {
        Utils.PrintAssetsPaths(__instance);
    }

}