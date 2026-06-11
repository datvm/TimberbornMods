namespace TestMod;

public static class Utils
{

    public static void PrintAssetsPaths(ModAssetBundleProvider provider)
    {
        Debug.Log(string.Join(Environment.NewLine, provider._assetPaths.Keys));
    }

}
