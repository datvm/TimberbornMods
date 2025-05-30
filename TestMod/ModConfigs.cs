
namespace TestMod;

[Context("Bootstrapper")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        MultiBind<IAssetProvider>().To<MyAsset>().AsSingleton();
    }
}

public class MyAsset : IAssetProvider
{
    public bool IsBuiltIn { get; }

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : UnityEngine.Object
    {
        return [];
    }

    public void Reset()
    {
        
    }

    public bool TryLoad(string path, Type type, out OrderedAsset orderedAsset)
    {
        orderedAsset = default;
        return false;
    }
}

public class ModStarter : IModStarter
{

    public static void Print(IEnumerable<IAssetProvider> assetProviders)
    {
        Debug.Log(assetProviders.Count());
        Debug.Log(string.Join(Environment.NewLine,
            assetProviders.Select(q => q.GetType().FullName)));
    }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TestMod));

        harmony.Patch(
            typeof(AssetLoader).GetConstructor([typeof(IEnumerable<IAssetProvider>)]),
            postfix: typeof(ModStarter).Method("Print"));

        harmony.PatchAll();
    }

}
