using Bindito.Core;

namespace ConfigurableGrowth;

[Context("MainMenu")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}
