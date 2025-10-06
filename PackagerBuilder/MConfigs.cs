global using PackagerBuilder.Services;
global using Object = UnityEngine.Object;

namespace PackagerBuilder;

public class MConfig : IModStarter, IModdableTimberbornRegistryWithPatchConfig
{
    public string? PatchCategory { get; }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance.AddConfigurator(this);
    }

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context.IsMenuContext())
        {
            configurator
                .BindSingleton<MenuButtonService>()
                .BindSingleton<PackagerModBuilder>()
                .BindSingleton<GoodBuilder>()
            ;

            configurator.TryBind<SpriteResizer>()?.AsSingleton();
            configurator.TryBind<SpriteFlipper>()?.AsSingleton();
            configurator.TryMultiBind<IDeserializer, UISpriteDeserializer>()?.AsSingleton();
            configurator.TryMultiBind<IDeserializer, FlippedSpriteDeserializer>()?.AsSingleton();
        }
        else if (context.IsBootstrapperContext())
        {
            configurator.Bind<PackagerPrefabProvider>().AsSingleton().AsExported();
            configurator.MultiBind<IAssetProvider>().ToExisting<PackagerPrefabProvider>();
        }
    }

}
