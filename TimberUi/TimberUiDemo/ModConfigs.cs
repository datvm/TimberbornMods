global using TimberUiDemo.Services;

namespace TimberUiDemo;

[Context("MainMenu")]
public class MainMenuConfigs : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        Debug.Log("Binding MainMenu");

        containerDefinition.Bind<MenuService>().AsSingleton();
    }
}
