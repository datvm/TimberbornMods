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
public class ModMainMenuConfig : CommonConfig
{
    public override void Configure()
    {
        base.Configure();

        this.MultiBindSingleton<IModUpdateNotifier, UpdateNotification>();
    }
}

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

            .BindSingleton<RenovationPriorityToggleGroupFactory>()
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

                    .AddDecorator<BuildingRenovationComponent, BuildingReinforcementComponent>() // Reinforcement 1-3
                    .AddDecorator<BuildingRenovationComponent, BuildingReinforceInvulComponent>() // Reinforcement Invul
                    .AddDecorator<BuildingRenovationComponent, ReinforceGearComponent>() // Metal Gear Solid
                    .AddDecorator<Workplace, ProductOverdriveComponent>() // Production Overdrive
                    .AddDecorator<Dwelling, DwellingDecorativeComponent>() // Dwelling Decorative

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
            .MultiBindAndBindSingleton<IRenovationProvider, DwellingDecorativeProvider>()
            .MultiBindAndBindSingleton<IRenovationProvider, ProductionOverdriveRenovationProvider>()

            .BindSingleton<RenovationDialogController>()
            .BindTransient<RenovationDialog>()

            .BindSingleton<BuildingRenovationElementDependencies>()
            .BindSingleton<DefaultRenovationProviderDependencies>()
            .BindSingleton<DefaultRenovationPanelFactory>()
        ;
    }
}
