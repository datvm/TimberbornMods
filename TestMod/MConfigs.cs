namespace TestMod;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {

    }
}

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<Test>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
    }
}

class Test(IAssetLoader assetLoader) : ILoadableSingleton
{

    public void Load()
    {
        var assets = (AssetLoader)assetLoader;

        foreach (var p in assets._assetProviders)
        {
            if (p is ModAssetBundleProvider bp)
            {
                Debug.Log(bp);


                foreach (var path in bp._assetPaths)
                {
                    Debug.Log(path.Key);
                }
            }
        }
    }

}