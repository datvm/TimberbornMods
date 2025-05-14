
namespace ScenarioEditor;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

public class CommonGameplayConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<ScenarioEventManager>().AsSingleton();

        this.BindTemplateModule()
            .AddDecorator<StartingLocationSpec, CustomStartComponent>()
            .Bind();
    }
}

[Context("MapEditor")]
public class ModMapEditorConfig : CommonGameplayConfig
{
    public override void Configure()
    {
        base.Configure();

        this.BindFragment<CustomStartFragment>();
    }
}

[Context("Game")]
public class ModGameConfig : CommonGameplayConfig
{
    public override void Configure()
    {
        base.Configure();

        Bind<ScenarioEventRegistry>().AsSingleton();
        Bind<CameraShakeService>().AsSingleton();
        Bind<MultiStartingLocationsService>().AsSingleton();

        MultiBind<IDevModule>().To<ScenarioEditorDevModule>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ScenarioEditor)).PatchAll();
    }

}