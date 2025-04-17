global using SaveEveryday.Services;

namespace SaveEveryday;

[Context("MainMenu")]
[Context("Game")]
public class SettingConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ServiceConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<SaveEverydayService>().AsSingleton();
        containerDefinition.Bind<AutosaveWarningService>().AsSingleton();
    }
}
