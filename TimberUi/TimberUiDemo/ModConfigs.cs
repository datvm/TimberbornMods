

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

    public class FragmentsProvider(DemoFragment coord) : EntityPanelFragmentProvider([coord]);

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