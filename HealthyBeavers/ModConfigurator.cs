using Bindito.Core;

namespace HealthyBeavers;
[Context("MainMenu")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}
