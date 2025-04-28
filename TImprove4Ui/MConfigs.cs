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
        Bind<MaterialCounterExpansionService>().AsSingleton();
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
        new Harmony(nameof(TImprove4Ui)).PatchAll();
    }
}
