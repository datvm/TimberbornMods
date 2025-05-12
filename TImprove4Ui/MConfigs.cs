global using TImprove4Ui.Patches;
global using TImprove4Ui.Services;

namespace TImprove4Ui;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        Bind<ScrollableEntityPanelService>().AsSingleton();
        Bind<PowerNetworkHighlighter>().AsSingleton();
        Bind<ToolPanelDescriptionMover>().AsSingleton();
        Bind<MaterialCounterService>().AsSingleton();
        Bind<ObjectSelectionService>().AsSingleton();

        Bind<BatchControlBoxService>().AsSingleton();
        Bind<WorkplacesBatchControlTabService>().AsSingleton();
    }
}

[Context("MapEditor")]
public class ModMapEditorConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        Bind<ScrollableEntityPanelService>().AsSingleton();
        Bind<ToolPanelDescriptionMover>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TImprove4Ui));
        harmony.PatchAll();
    }
}
