global using Gate.Components;
global using Gate.UI;
global using Gate.Services;

namespace Gate;

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<AutoGateService>()

            .BindFragment<GateEntityPanelFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<GateComponentSpec, GateComponent>()
            );
    }

}
