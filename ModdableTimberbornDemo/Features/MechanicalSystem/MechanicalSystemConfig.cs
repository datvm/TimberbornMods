namespace ModdableTimberbornDemo.Features.MechanicalSystem;

public class MechanicalSystemConfig : IModdableTimberbornRegistryConfig
{

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindFragment<DemoMechanicalSystemFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<MechanicalNode, DemoAdditiveMechanicalSystemModifier>()
                .AddDecorator<MechanicalNode, DemoMultiplicativeMechanicalSystemModifier>()
                .AddDecorator<MechanicalNode, DemoForceMechanicalSystemModifier>()
            )
        ;
    }

}
