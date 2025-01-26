global using Bindito.Core;
global using LateGamePower.UI;
global using Timberborn.EntityPanelSystem;
global using Timberborn.TemplateSystem;
global using Timberborn.Workshops;

namespace LateGamePower;

[Context("MainMenu")]
public class MainMenuModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}

[Context("Game")]
public class GameModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
        containerDefinition.Bind<ScienceToPowerService>().AsSingleton();
        containerDefinition.Bind<PanelFragment>().AsTransient();
        containerDefinition.Bind<PowerPanelFragment>().AsTransient();
        containerDefinition.Bind<ScienceToPowerFragment>().AsTransient();

        containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
        containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
        var builder = new TemplateModule.Builder();
        builder.AddDecorator<MechanicalNode, ScienceToPowerComponent>();
        return builder.Build();
    }

    class EntityPanelModuleProvider(ScienceToPowerFragment scienceToPowerFragment) : IProvider<EntityPanelModule>
    {
        
        public EntityPanelModule Get()
        {
            EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
            builder.AddMiddleFragment(scienceToPowerFragment);
            return builder.Build();
        }
    }
}