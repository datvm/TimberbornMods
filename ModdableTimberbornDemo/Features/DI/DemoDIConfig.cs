namespace ModdableTimberbornDemo.Features.DI;

public class DemoDIConfig : IModdableTimberbornRegistryConfig
{

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            // Spec
            .MultiBindSingleton<ISpecServiceFrontRunner, DemoSpecFrontRunner>()
            .MultiBindSingleton<ISpecServiceTailRunner, DemoSpecTailRunner>()
            .MultiBindSingleton<ISpecModifier, DemoSpecModifier1>()
            .MultiBindSingleton<ISpecModifier, DemoSpecModifier2>()

            .MultiBindSingleton<ISpecModifier, DemoNewItemFactionSpecModifier>()
            .MultiBindSingleton<ISpecModifier, DemoNewItemGoodSpecModifier>()

            // Prefab
            .MultiBindSingleton<IPrefabGroupServiceTailRunner, DemoPrefabTailRunner>()
            .MultiBindSingleton<IPrefabModifier, DemoDamPrefabModifier1>()
            .MultiBindSingleton<IPrefabModifier, DemoDamPrefabModifier2>()
        ;
            
    }

}
