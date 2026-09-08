namespace BeavlineLogistics;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony
{

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<BuildingInventoryProvider>()
            .BindSingleton<BeavlineService>()
            .BindSingleton<StockpileBalancerService>()

            .MultiBindSingleton<IRenovationProvider, BeavlineInRenovationProvider>()
            .MultiBindSingleton<IRenovationProvider, BeavlineOutRenovationProvider>()
            .MultiBindSingleton<IRenovationProvider, BeavlineBalancerRenovationProvider>()
            .MultiBindSingleton<IRenovationProvider, BeavlineOutSpeedRenovationProvider>()

            .BindFragment<BeavlineFragment>() 
            .BindTransient<GoodFilterDialog>()
            .BindTransient<BeavlineNodePanel>()
            .BindFragment<StockpileBalancerFragment>()

            .MultiBindSingleton<IDevModule, BeavlineDevModule>()

            .BindTemplateModule(h => h
                .AddDecorator<BuildingSpec, BeavlineComponent>()
                .AddDecorator<BeavlineComponent, BeavlineOutputComponent>()
                .AddDecorator<StockpileSpec, BeavlineBalancerComponent>()
            )
        ;
    }

}
