using Bindito.Core;

namespace CutAllTrees;

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
        containerDefinition.Bind<MarkTreeService>().AsSingleton();
    }
}
