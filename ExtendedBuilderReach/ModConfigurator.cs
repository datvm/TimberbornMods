using Bindito.Core;

namespace ExtendedBuilderReach;
[Context("MainMenu")]
public class MainMenuModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}
