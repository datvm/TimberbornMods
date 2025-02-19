global using TimberUiDemo.Services;
global using Timberborn.BottomBarSystem;
global using TimberUiDemo.UI;
using TimberUi.CommonProviders;
using TimberUi.Extensions;

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

    public class FragmentsProvider(CoordinatesFragment coord) : EntityPanelFragmentProvider([coord]);

    public override void Configure()
    {
        Bind<DevModeButton>().AsSingleton();
        Bind<GameService>().AsSingleton();

        // This method already bind fragments as Singleton, no need to register them anymore
        this.BindFragments<FragmentsProvider>();
        this.BindFragments<ColorfulFragmentsProvider>();

        MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<DevModeButton>>().AsSingleton();
        MultiBind<IDevModule>().To<PrintVisualTreeDevModule>().AsSingleton();
    }

}