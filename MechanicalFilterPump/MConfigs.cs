global using MechanicalFilterPump.UI;
global using Timberborn.WaterBuildings;

namespace MechanicalFilterPump;

[Context("Game")]
public class ModGameConfigurator : Configurator
{
    public override void Configure()
    {
        this.BindFragments<EntityPanelFragmentProvider<MechanicalFilterPumpFragment>>();
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<WaterMoverSpec, MechanicalFilterPumpComponent>();
            return b.Build();
        }).AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(MechanicalFilterPump)).PatchAll();
    }

}