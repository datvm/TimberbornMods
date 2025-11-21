global using ConfigurableTopBar.Services;
global using ConfigurableTopBar.Models;
global using ConfigurableTopBar.UI;
global using ConfigurableTopBar.Helpers;

global using ModdableTimberborn.DependencyInjection;

namespace ConfigurableTopBar;

public class ConfigurableTopBarConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{

    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<TopBarConfigProvider>();

        switch (context)
        {
            case ConfigurationContext.MainMenu:
                BindMenu(configurator);
                break;
            case ConfigurationContext.Game:
                BindGame(configurator);
                break;
        }
    }

    void BindMenu(Configurator configurator)
    {
        configurator
            .BindSingleton<MSettings>()
            .BindSingleton<GoodSpriteProvider>()
            
            .MultiBindSingleton<IModSettingElementFactory, ConfigurableTopBarSettingFactory>()
            .BindTransient<ConfigurableTopBarPanel>()
            .BindTransient<EditableGoodGroupPanel>()
            .BindTransient<EditableGoodPanel>()
            .BindTransient<SelectIconDialog>()
        ;

        configurator.TryBind<SpriteResizer>()?.AsSingleton();
        configurator.TryBind<SpriteFlipper>()?.AsSingleton();
        configurator.TryMultiBind<IDeserializer, UISpriteDeserializer>()?.AsSingleton();
        configurator.TryMultiBind<IDeserializer, FlippedSpriteDeserializer>()?.AsSingleton();
    }

    void BindGame(Configurator configurator)
    {
        configurator
            .MultiBindSingleton<ISpecServiceTailRunner, TopBarProviderInitiator>()
            .BindSingleton<GoodGroupModifier>() // Do NOT bind as Spec Modifier, it's executed by the TopBarProviderInitator
            .BindSingleton<GoodSpecModifier>()
        ;
    }

}
