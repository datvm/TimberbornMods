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

            .BindSingleton<CollapsibleEntityPanelService>()
        ;
    }
}
