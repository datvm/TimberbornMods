global using TImprove4Ui.Patches;
global using TImprove4Ui.Services;

namespace TImprove4Ui;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<MSettings>();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()

            .BindSingleton<ShadowService>()

            .BindSingleton<ScrollableEntityPanelService>()
            .BindSingleton<ToolPanelDescriptionMover>()
            .BindSingleton<MaterialCounterService>()
            .BindSingleton<ObjectSelectionService>()
            .BindSingleton<MaterialFinderService>()

            .BindSingleton<BatchControlBoxService>()
            .BindSingleton<WorkplacesBatchControlTabService>()

            .BindSingleton<FactionNeedSpecService>()
            .BindTemplateModule(h => h
                .AddDecorator<AreaNeedApplier, NeedApplierDescriber>()
                .AddDecorator<WorkshopRandomNeedApplier, NeedApplierDescriber>()
            )
        ;
    }
}

[Context("MapEditor")]
public class ModMapEditorConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()

            .BindSingleton<ShadowService>()
            .BindSingleton<ScrollableEntityPanelService>()
            .BindSingleton<ToolPanelDescriptionMover>()
        ;
    }
}

public class ModStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TImprove4Ui));

        harmony.Patch(
            typeof(TopBarCounterRow).GetConstructors().First(),
            postfix: typeof(MaterialCounterPatches).Method(nameof(MaterialCounterPatches.AddCounterEvents))
        );

        harmony.PatchAll();
    }
}
