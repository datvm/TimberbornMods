using ScenarioEditor.UI.ScenarioEvents;

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
        this
            .BindSingleton<MSettings>()
            .BindSingleton<ScenarioEventManager>()
            
            .TryBindingCameraShake(false)

            .BindTemplateModule(h => h
                .AddDecorator<StartingLocationSpec, CustomStartComponent>()
            )
        ;
    }
}

[Context("MapEditor")]
public class ModMapEditorConfig : CommonGameplayConfig
{
    public override void Configure()
    {
        base.Configure();

        this
            .BindFragment<CustomStartFragment>()
            .BindSingleton<PrefabNameProvider>()
            .BindSingleton<ScenarioEventsController>()
            .BindSingleton<ScenarioEventsDialog>()
        ;
    }
}

[Context("Game")]
public class ModGameConfig : CommonGameplayConfig
{
    public override void Configure()
    {
        base.Configure();

        this
            .BindSingleton<ScenarioEventRegistry>()
            .BindSingleton<MultiStartingLocationsService>()
            .BindSingleton<TriggerAreaService>()
        ;
    }
}

public class ModStarter : IModStarter
{

    public static string ModPath { get; private set; } = "";

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;

        new Harmony(nameof(ScenarioEditor)).PatchAll();
    }

}