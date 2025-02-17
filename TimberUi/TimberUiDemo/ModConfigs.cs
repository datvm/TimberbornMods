global using TimberUiDemo.Services;
global using Timberborn.BottomBarSystem;
global using TimberUiDemo.UI;

namespace TimberUiDemo;

[Context("MainMenu")]
public class MainMenuConfigs : Configurator
{
    public override void Configure()
    {
        Bind<MenuService>().AsSingleton();
    }
}

[Context("Game")]
public class GameConfigs : Configurator
{

    public override void Configure()
    {
        Bind<DevModeButton>().AsSingleton();
        Bind<GameService>().AsSingleton();

        MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<DevModeButton>>().AsSingleton();
    }

}