namespace BuildingHP;

public class MConfigs : BaseModdableTimberbornConfiguration
{
    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseBonusTracker()
        ;
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        var isMenu = context.IsMenuContext();
        var isGame = context.IsGameContext();

        if (!isMenu && !isGame) { return; }

        configurator
            .BindSingleton<BuildingMaterialDurabilityService>()
            .BindSingleton<MSettings>()
        ;

        if (!context.IsGameContext()) { return; }

        BindGameServices(configurator);
        BindRenovations(configurator);
    }

    static void BindGameServices(Configurator configurator)
    {
        configurator
            .BindSingleton<BuildingHPRegistry>()
            .BindSingleton<BuildingHPService>()
            .BindSingleton<BuildingRepairService>()

            .BindSingleton<RenovationPriorityToggleGroupFactory>()
            .BindOrderedFragment<BuildingHPFragment>()
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
                    .AddDecorator<ProductOverdriveComponent, ProductOverdriveBonusComponent>()
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

    static void BindRenovations(Configurator configurator)
    {
        configurator
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
