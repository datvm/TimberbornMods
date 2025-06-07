namespace ConfigurableFaction;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()

            .BindSingleton<FactionInfoService>()
            .BindSingleton<PersistentService>()
            .BindSingleton<FactionOptionsProvider>()
            .BindSingleton<FactionOptionsService>()
        ;

        BindUI();
        BindSpriteOperations();
    }

    void BindUI()
    {
        Bind<FactionSettingPanel>().AsTransient();
        Bind<FactionBuildingsPanel>().AsTransient();
        Bind<SettingDialog>().AsTransient();
        MultiBind<IModSettingElementFactory>().To<SettingDialogModSettingFactory>().AsSingleton();
    }

    void BindSpriteOperations()
    {
        // From SpriteOperationsConfigurator
        Bind<SpriteResizer>().AsSingleton();
        Bind<SpriteFlipper>().AsSingleton();
        MultiBind<IDeserializer>().To<UISpriteDeserializer>().AsSingleton();
        MultiBind<IDeserializer>().To<FlippedSpriteDeserializer>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableFaction)).PatchAll();
    }

}
