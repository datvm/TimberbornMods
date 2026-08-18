namespace Crane;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    protected override void ConfigureRegistry(ModdableTimberbornRegistry registry)
        => registry.UseEntityTracker()
            .TryTrack<CraneComponent>()
            .TryTrack<CraneSectionComponent>();

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator.BindTemplateModule(h => h
            .AddDecorator<CraneWorkshop, Emptiable>(false)
            .AddDecorator<CraneWorkshop, InventoryGoodConsumptionBlocker>(false)
            .AddDecorator<CraneWorkshop, FillInputHaulBehaviorProvider>(false)

            .AddDecorator<CraneWorkshop, CraneWorkWorkplaceBehavior>()
            .AddDecorator<CraneWorkshop, FillInputWorkplaceBehavior>(false)
            .AddDecorator<CraneWorkshop, RemoveUnwantedStockWorkplaceBehavior>(false)
            .AddDecorator<CraneWorkshop, EmptyInventoriesWorkplaceBehavior>(false)
            .AddDecorator<CraneWorkshop, WaitInsideIdlyWorkplaceBehavior>(false)

            .AddDecorator<Worker, CraneWorkExecutor>()
        );

        configurator.MultiBind<TemplateModule>().ToProvider<CraneTemplateModuleProvider>().AsSingleton();
    }

    class CraneTemplateModuleProvider(CraneInventoryInitializer inventoryInitializer) : IProvider<TemplateModule>
    {
        public TemplateModule Get()
        {
            var builder = new TemplateModule.Builder();
            builder.AddDedicatedDecorator(inventoryInitializer);
            return builder.Build();
        }

    }
}
