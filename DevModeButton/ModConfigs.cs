global using Timberborn.BottomBarSystem;

namespace DevModeButton;

[Context("Game")]
public class GameConfigs : Configurator
{

    public override void Configure()
    {
        Bind<DevModeButton>().AsSingleton();
        MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<DevModeButton>>().AsSingleton();
    }

}