namespace Packager;

[Context("Bootstrapper")]
public class ModBootstraperConfig : Configurator
{

    public override void Configure()
    {
        MultiBind<IAssetProvider>().To<PackagerPrefabProvider>().AsSingleton();
        MultiBind<ISpecServiceFrontRunner>().To<PackagedGoodProvider>().AsSingleton().AsExported();
        Bind<PackagerOverlayIconMaker>().AsSingleton().AsExported();
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<PackagerManufactorySpec, Manufactory>();
            return b.Build();
        }).AsSingleton();
    }

}