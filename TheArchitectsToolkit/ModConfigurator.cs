namespace TheArchitectsToolkit;

[Context("MainMenu")]
[Context("MapEditor")]
public class MainMenuModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
        containerDefinition.Bind<ToolkitService>().AsSingleton();
    }
}

[Context("Game")]
public class GameModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}