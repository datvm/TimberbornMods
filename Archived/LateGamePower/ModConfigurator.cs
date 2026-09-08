global using LateGamePower.UI;
global using Timberborn.EntityPanelSystem;

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
    }

    class EntityPanelModuleProvider(ScienceToPowerFragment scienceToPowerFragment) : IProvider<EntityPanelModule>
    {
        
        public EntityPanelModule Get()
        {
            EntityPanelModule.Builder builder = new();
            builder.AddMiddleFragment(scienceToPowerFragment);
            return builder.Build();
        }
    }
}