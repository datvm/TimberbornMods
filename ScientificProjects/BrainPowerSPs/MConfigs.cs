namespace BrainPowerSPs;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony
{
    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection()
            .UseMechanicalSystem()
            .TryTrack<WaterWheelPowerSPComponent>()
            .TryTrack<WindPowerSPComponent>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindScientificProjectCostProvider<PowerSPCostProvider>()

            .BindScientificProjectListener<WaterWheelPowerSPService>(true)
            .BindScientificProjectListener<SparePowerToScienceConverter>(true)
            .BindScientificProjectListener<WindPowerSPService>(true)

            .MultiBindSingleton<IPrefabModifier, PowerSPPrefabModifier>()

            .BindTemplateModule(h => h
                .AddDecorator<WaterPoweredGeneratorSpec, WaterWheelPowerSPComponent>()
                .AddDecorator<WindPoweredGeneratorSpec, WindPowerSPComponent>()
                .AddDecorator<ManufactorySpec, SparePowerDescriber>()
            )
        ;
    }
}
