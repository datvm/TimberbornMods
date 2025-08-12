using TImprove4UX.Services;

namespace TImprove4UX;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()
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
        ;
    }
}
