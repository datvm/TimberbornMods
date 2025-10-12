namespace BeavlineLogistics;

public class MConfigs : BaseModdableTimberbornConfiguration
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

            .BindFragment<BeavlineFragment>() 
            .BindTransient<GoodFilterDialog>()
            .BindTransient<BeavlineNodePanel>()

            .BindTemplateModule(h => h
                .AddDecorator<BuildingSpec, BeavlineComponent>()
                .AddDecorator<BeavlineComponent, BeavlineOutputComponent>()
                .AddDecorator<StockpileSpec, BeavlineBalancerComponent>()
            )
        ;
    }

}
