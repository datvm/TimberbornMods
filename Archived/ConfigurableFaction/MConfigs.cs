namespace ConfigurableFaction;

public class BaseConfig : Configurator
{
    public override void Configure()
    {
        this
            .MassRebind(r =>
                r.Replace<FactionSpecService, ConfigurableFactionSpecService>()
            )

            .MultiBindSingleton<IAssetProvider, ConfigurableFactionBlueprintProvider>()
        ;
    }
}

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{

    public override void Configure()
    {
        Bind<PersistentService>().AsSingleton().AsExported();
        Bind<FactionOptionsProvider>().AsSingleton().AsExported(); 

        this
            .MultiBindSingleton<IAssetProvider, ConfigurableFactionBlueprintProvider>();
    }

}

[Context("MainMenu")]
public class ModMenuConfig : BaseConfig
{
    public override void Configure()
    {
        base.Configure();

        this
            .BindSingleton<MSettings>()

            .BindSingleton<FactionInfoService>()
            .BindSingleton<FactionOptionsService>()
            
            // Export feature
            .BindSingleton<ConflictModChecker>()

            .MultiBindSingleton<IModUpdateNotifier, UpdateNotifier>()
        ;

        BindUI();
        BindSpriteOperations();

        if (MStarter.HasTimberModBuilder)
        {
            this.BindSingleton<ExportToModService>();
        }
    }

    void BindUI()
    {
        this
            .BindTransient<FactionSettingPanel>()
            .BindTransient<SettingDialog>()

            .MultiBindSingleton<IModSettingElementFactory, SettingDialogModSettingFactory>()
        ;

        // Bind the panels
        foreach (var t in FactionSettingPanel.PanelTypes)
        {
            this.Bind(t).AsTransient();
        }
    }

    void BindSpriteOperations()
    {
        // From SpriteOperationsConfigurator
        this
            .BindSingleton<SpriteResizer>()
            .BindSingleton<SpriteFlipper>()

            .MultiBindSingleton<IDeserializer, UISpriteDeserializer>()
            .MultiBindSingleton<IDeserializer, FlippedSpriteDeserializer>()
        ;
    }

}

[Context("Game")]
public class ModGameConfig : BaseConfig
{
    public override void Configure()
    {
        base.Configure();
    }
}
