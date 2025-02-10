namespace QuickStart;

[Context("MainMenu")]
public class MainMenuConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}