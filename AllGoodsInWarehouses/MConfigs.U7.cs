global using ModdableTimberborn.DependencyInjection;
global using AllGoodsInWarehouses.Services;

namespace AllGoodsInWarehouses;

public class AllGoodsInWarehousesConfigs : BaseModdableTimberbornConfiguration
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
            .BindSpecModifier<GoodSpecModifier>()
        ;
    }
}
