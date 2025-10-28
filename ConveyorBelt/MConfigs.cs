namespace ConveyorBelt;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony
{

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<ConveyorBeltService>()

            .BindPrefabModifier<ConveyorBeltPrefabModifier>()

            .BindTemplateModule(h => h
                .AddDecorator<ConveyorBeltSpec, ConveyorBeltComponent>()
                .AddDecorator<ConveyorBeltSpec, MechanicalBuildingSpec>()
            )
        ;
    }
}
