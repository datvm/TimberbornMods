namespace ThePostOffice;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator.BindTemplateModule(h => h
            .AddDecorator<PostOfficeComponent, Emptiable>(false)
            .AddDecorator<PostOfficeComponent, InventoryGoodConsumptionBlocker>(false)
            .AddDecorator<PostOfficeComponent, FillInputHaulBehaviorProvider>(false)
            .AddDecorator<PostOfficeComponent, PostOfficeWorkWorkplaceBehavior>()
            .AddDecorator<PostOfficeComponent, FillInputWorkplaceBehavior>(false)
            .AddDecorator<PostOfficeComponent, RemoveUnwantedStockWorkplaceBehavior>(false)
            .AddDecorator<PostOfficeComponent, EmptyInventoriesWorkplaceBehavior>(false)
            .AddDecorator<Worker, PostOfficeWorkExecutor>()
        );

        configurator.MultiBind<TemplateModule>().ToProvider<PostOfficeTemplateModuleProvider>().AsSingleton();
    }

    class PostOfficeTemplateModuleProvider(PostOfficeInventoryInitializer inventoryInitializer) : IProvider<TemplateModule>
    {
        public TemplateModule Get()
        {
            var builder = new TemplateModule.Builder();
            builder.AddDedicatedDecorator(inventoryInitializer);
            return builder.Build();
        }
    }
}
