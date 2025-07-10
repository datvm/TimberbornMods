namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));

        var method = typeof(AssetLoader).Method("LoadAll");
        var target = method.MakeGenericMethod([typeof(TextAsset)]);

        h.Patch(target, postfix: typeof(MStarter).Method(nameof(LoadAllTextAsset)));
    }

    public static void LoadAllTextAsset(string path, object __result)
    {
        if (__result is IEnumerable<LoadedAsset<TextAsset>> textAssets)
        {
            Debug.Log($"This is the actual text assets, at path {path}");
        }
        else
        {
            Debug.Log($"Not Text assets at {path}, result: {__result.GetType().FullName}");
        }
    }


}
