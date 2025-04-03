namespace TheArchitectsToolkit.Services;

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class AllContextModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
        containerDefinition.Bind<ToolkitService>().AsSingleton();
    }
}