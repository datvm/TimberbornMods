namespace ConfigurableAutoSave;

[Context("Game")]
[Context("MainMenu")]
public class SettingsConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class GameServiceConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ConfigurableAutoSaveService>().AsSingleton();
    }
}
