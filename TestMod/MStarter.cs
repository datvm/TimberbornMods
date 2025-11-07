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

    [HarmonyPrefix, HarmonyPatch(typeof(SpecService), nameof(SpecService.Load))]
    public static void Prefix(SpecService __instance)
    {
        var bundles = __instance._blueprintFileBundleLoader.GetBundles(SpecService.BlueprintsPath);

        foreach (var bundle in bundles)
        {
            Debug.Log($"Bundle {bundle.Name} with path {bundle.Path} is from:\r\n"
                + string.Join("\r\n", bundle.Sources.Select(q => "- " + q)));
        }
    }

}
