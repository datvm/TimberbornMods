global using Gate.Components;
global using Gate.UI;

namespace Gate;

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        this.BindTemplateModule(h => h.AddDecorator<GateComponentSpec, GateComponent>());
        this.BindFragment<GateEntityPanelFragment>();
    }

}

public class ModStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(Gate)).PatchAll();
    }
}