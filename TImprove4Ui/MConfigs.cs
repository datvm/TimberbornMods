global using TImprove4Ui.Patches;
global using TImprove4Ui.Services;
global using TImprove4Ui.UI;
global using TImprove4Ui.Components;
global using System.Reflection.Emit;

namespace TImprove4Ui;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()
            .BindSingleton<MenuLoaderService>()

            .MultiBindSingleton<IModUpdateNotifier, ModUpdateNotifier>()
        ;
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()
            .BindSingleton<MenuLoaderService>()

            .BindSingleton<ShadowService>()

            .BindSingleton<StaticSingletonsService>()

            .BindSingleton<ScrollableEntityPanelService>()
            .BindSingleton<ToolPanelDescriptionMover>()
            .BindSingleton<MaterialCounterService>()
            .BindSingleton<ObjectSelectionService>()
            .BindSingleton<MaterialFinderService>()

            .BindSingleton<BatchControlBoxService>()
            .BindSingleton<WorkplacesBatchControlTabService>()

            .BindSingleton<SceneService>()

            // Pause status icon
            .BindSingleton<PauseStatusIconRegistry>()
            .BindFragment<PauseStatusIconFragment>()

            .BindSingleton<FactionNeedSpecService>()
            .BindTemplateModule(h => h
                .AddDecorator<AreaNeedApplier, NeedApplierDescriber>()
                .AddDecorator<WorkshopRandomNeedApplier, NeedApplierDescriber>()
                
                .AddDecorator<StatusSubject, StatusTracker>()
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
