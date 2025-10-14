namespace BlockObjectDebugger;

public class MConfigs : BaseModdableTimberbornConfiguration
{

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context.IsBootstrapperContext())
        {
            configurator
                .MultiBindSingleton<IAssetProvider, BuildingsPrefabProvider>()
            ;
        }

        if (!context.IsGameplayContext()) { return; }

        configurator
            .BindSingleton<BlockObjectDebuggerMaterialService>()

            .MultiBindSingleton<ISpecModifier, BuildingsSpecModifier>()

            .BindOrderedFragment<BlockObjectFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<BlockObjectSpec, BlockObjectDebuggerComponent>()
            )
        ;
    }
}
