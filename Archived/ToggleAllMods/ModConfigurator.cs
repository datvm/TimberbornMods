global using ToggleAllMods.UI;

namespace ToggleAllMods;
[Context("MainMenu")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
        containerDefinition.Bind<ModManagerBoxUI>().AsSingleton();
    }
}
