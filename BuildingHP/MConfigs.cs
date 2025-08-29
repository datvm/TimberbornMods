namespace BuildingHP;

public class CommonConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<BuildingMaterialDurabilityService>()
            .BindSingleton<MSettings>()
        ;
    }
}

[Context("MainMenu")]
public class ModMainMenuConfig : CommonConfig { }

[Context("Game")]
public class ModGameConfig : CommonConfig
{
    public override void Configure()
    {
        base.Configure();

        BindRenovations();

        this
            .BindSingleton<BuildingHPRegistry>()
            .BindSingleton<BuildingHPService>()
            .BindSingleton<BuildingRepairService>()

            .BindFragment<BuildingHPFragment>()
            .BindSingleton<BuildingHPFragmentMover>()            
            .BindFragment<RenovationStockpileFragment>()

            .MultiBindSingleton<IDevModule, HPDevModule>()

            .BindTemplateModule(h =>
            {
                h
                    // HP
                    .AddDecorator<BuildingSpec, BuildingHPComponentSpec>()
                    .AddDecorator<BuildingHPComponentSpec, BuildingHPComponent>()
                    .AddDecorator<BuildingHPComponentSpec, BuildingHPWarningComponent>()
                    .AddDecorator<BuildingHPComponentSpec, BuildingHPRepairComponent>()

                    // Renovations & its effects
                    .AddDecorator<BuildingHPComponentSpec, BuildingRenovationComponent>()

                    .AddDecorator<BuildingRenovationComponent, BuildingReinforcementComponent>()
                    .AddDecorator<BuildingRenovationComponent, BuildingReinforceInvulComponent>()
                    .AddDecorator<BuildingRenovationComponent, ReinforceGearComponent>()

                    // Stockpile for Renovation
                    .AddDecorator<StockpileSpec, BuildingRenovationStockpileComponent>()
                ;

                if (MSettings.EnableMaterialDurabilityValue)
                {
                    h.AddDecorator<BuildingHPComponentSpec, BuildingMaterialDurabilityComponent>();
                }

                if (MSettings.EnableMaintenanceValue)
                {
                    h.AddDecorator<BuildingHPComponentSpec, HPMaintenanceComponentSpec>();
                    h.AddDecorator<HPMaintenanceComponentSpec, HPMaintenanceComponent>();
                }

                return h;
            })
        ;
    }

    void BindRenovations()
    {
        this
            .BindSingleton<BuildingRenovationService>()

            .BindSingleton<RenovationSpecService>()
            .BindSingleton<RenovationRegistry>()

            .BindSingleton<BuildingRenovationDependencies>()
            .MultiBindAndBindSingleton<IRenovationProvider, RepairRenovationProvider>()
            .MultiBindAndBindSingleton<IRenovationProvider, Reinforce1RenovationProvider>()
            .MultiBindAndBindSingleton<IRenovationProvider, Reinforce2RenovationProvider>()
            .MultiBindAndBindSingleton<IRenovationProvider, Reinforce3RenovationProvider>()
            .MultiBindAndBindSingleton<IRenovationProvider, ReinforceInvulRenovationProvider>()
            .MultiBindAndBindSingleton<IRenovationProvider, ReinforceGearRenovationProvider>()

            .BindSingleton<RenovationDialogController>()
            .BindTransient<RenovationDialog>()

            .BindSingleton<BuildingRenovationElementDependencies>()
            .BindSingleton<DefaultRenovationProviderDependencies>()
            .BindSingleton<DefaultRenovationPanelFactory>()
        ;
    }
}
