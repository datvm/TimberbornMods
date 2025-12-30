global using WarningsBeGone.Components;
global using WarningsBeGone.Services;
global using WarningsBeGone.UI;

namespace WarningsBeGone;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<StatusHidingService>()

            .BindFragment<StatusHidingFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<StatusSubject, StatusHidingComponent>()
            )
        ;
    }
}
