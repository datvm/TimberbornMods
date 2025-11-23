global using GateV1.Components;
global using GateV1.Services;
global using GateV1.UI;

namespace GateV1;

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
