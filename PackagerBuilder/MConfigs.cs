global using PackagerBuilder.Services;
global using PackagerBuilder.Services.GoodProviders;
global using PackagerBuilder.UI;
global using Object = UnityEngine.Object;

namespace PackagerBuilder;

public class MConfig : BaseModdableTimberbornConfigurationWithHarmony
{
    public static bool? HasPackagerMod;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context.IsMenuContext())
        {
            configurator.MultiBindSingleton<IModUpdateNotifier, ModUpdateNotifier>();

            if (HasPackagerMod != true)
            {
                BindBuilder(configurator);
            }
        }
        else if (context.IsBootstrapperContext())
        {
            configurator.Bind<PackagerPrefabProvider>().AsSingleton().AsExported();
            configurator.MultiBind<IAssetProvider>().ToExisting<PackagerPrefabProvider>();
        }
    }

    static void BindBuilder(Configurator configurator)
    {
        configurator
            .BindSingleton<MenuButtonService>()
            .BindSingleton<PackagerModBuilder>()
            .BindSingleton<GoodBuilder>()
            .BindSingleton<PackagerOverlayIconMaker>()

            .BindTransient<PackagerOptionDialog>()

            .MultiBindSingleton<IGoodBuilderProvider, Package10Provider>()
            .MultiBindSingleton<IGoodBuilderProvider, CrateGoodProvider>()
            .MultiBindSingleton<IGoodBuilderProvider, BarrelGoodProvider>()
        ;

        configurator.TryBind<SpriteResizer>()?.AsSingleton();
        configurator.TryBind<SpriteFlipper>()?.AsSingleton();
        configurator.TryMultiBind<IDeserializer, UISpriteDeserializer>()?.AsSingleton();
        configurator.TryMultiBind<IDeserializer, FlippedSpriteDeserializer>()?.AsSingleton();
    }

}
