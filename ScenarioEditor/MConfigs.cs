
namespace ScenarioEditor;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("MapEditor")]
public class ModMapEditorConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<ScenarioEventManager>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<ScenarioEventManager>().AsSingleton();

        // Should only available during gameplay
        Bind<ScenarioEventRegistry>().AsSingleton();
        Bind<CameraShakeService>().AsSingleton();

        MultiBind<IDevModule>().To<ScenarioEditorDevModule>().AsSingleton();
    }
}
