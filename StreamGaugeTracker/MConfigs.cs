global using StreamGaugeTracker.Components;
global using StreamGaugeTracker.Services;
global using StreamGaugeTracker.UI;

namespace StreamGaugeTracker;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
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

            .BindSingleton<StreamGaugeTrackerService>()

            .BindSingleton<StreamGaugeUIAdder>()
            .BindFragment<StreamGaugeTrackerFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<StreamGauge, StreamGaugeTrackerComponent>()
            )
        ;
    }

}
