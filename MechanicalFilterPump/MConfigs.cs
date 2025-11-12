global using MechanicalFilterPump.Components;
global using MechanicalFilterPump.UI;
global using ModdableTimberborn.MechanicalSystem;

namespace MechanicalFilterPump;

public class MechanicalFilterPumpConfig : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseMechanicalSystem();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindFragment<MechanicalFilterPumpFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<WaterMoverSpec, MechanicalFilterPumpComponent>()
                .AddDecorator<WaterMoverSpec, MechanicalFilterPumpPower>()
            )
        ;
    }
}
