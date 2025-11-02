namespace TImprove4UX;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()

            .MultiBindSingleton<IModUpdateNotifier, ModUpdateNotification>()
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

            .BindSingleton<UndoBuildingService>()
            .BindSingleton<AlternateDeleteObjectTool>()
            .BindSingleton<RecentToolService>()
            .BindSingleton<DynamiteDestructionService>()
#if TIMBERU7
            .BindSingleton<BuildingCopyService>()
#endif

            .BindSingleton<CollapsibleEntityPanelService>()

            .BindSingleton<WorkerIdleWarningService>()
            .BindFragment<IdleWarningFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<Workplace, WorkerIdleWarningComponent>()
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
            .BindSingleton<RecentToolService>()
#if TIMBERU7
            .BindSingleton<BuildingCopyService>()
#endif
            .BindSingleton<CollapsibleEntityPanelService>()
        ;
    }
}