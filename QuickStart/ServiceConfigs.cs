global using QuickStart.Services;

namespace QuickStart;

[Context("MainMenu")]
public class MainMenuConfig : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<AutoLoadService>().AsSingleton();
        Bind<QuickMapEditorService>().AsSingleton();
    }
}