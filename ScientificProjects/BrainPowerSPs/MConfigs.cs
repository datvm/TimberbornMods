namespace BrainPowerSPs;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseMechanicalSystem()
            .TryTrack<WaterWheelPowerSPComponent>()
            .TryTrack<WindPowerSPComponent>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindScientificProjectCostProvider<PowerSPCostProvider>()

            .BindScientificProjectListener<WaterWheelPowerSPService>(true)
            .BindScientificProjectListener<SparePowerToScienceConverter>(true)
            .BindScientificProjectListener<WindPowerSPService>(true)

            .BindTemplateModifier<PowerSPTemplateModifier>()

            .BindTemplateModule(h => h
                .AddDecorator<WaterPoweredGeneratorSpec, WaterWheelPowerSPComponent>()
                .AddDecorator<WindPoweredGeneratorSpec, WindPowerSPComponent>()
                .AddDecorator<ManufactorySpec, SparePowerDescriber>()
            )
        ;
    }
}
