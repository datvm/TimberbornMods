global using ModdableTimberborn.DependencyInjection;
global using AllGoodsInWarehouses.Services;

namespace AllGoodsInWarehouses;
public class AllGoodsInWarehousesConfigs : BaseModdableTimberbornConfiguration, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindSpecModifier<GoodSpecModifier>()
        ;
    }
}
