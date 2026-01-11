global using ConstructionNotifier.Components;
global using ConstructionNotifier.Services;
global using ConstructionNotifier.UI;

namespace ConstructionNotifier;

[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<ConstructionSiteNotifierService>()

            .BindSingleton<ConstructionSiteFragmentAppender>()

            .BindTemplateModule(h => h
                .AddDecorator<ConstructionSite, ConstructionSiteNotifier>()
            )
        ;
    }

}
