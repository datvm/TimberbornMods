namespace ThePostOffice;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator.BindTemplateModule(h => h
            .AddDecorator<PostOfficeSender, Emptiable>(false)
            .AddDecorator<PostOfficeSender, InventoryGoodConsumptionBlocker>(false)
            .AddDecorator<PostOfficeSender, FillInputHaulBehaviorProvider>(false)
            .AddDecorator<PostOfficeSender, PostOfficeWorkWorkplaceBehavior>()
            .AddDecorator<PostOfficeSender, PostOfficeStatus>()
            .AddDecorator<PostOfficeSender, FillInputWorkplaceBehavior>(false)
            .AddDecorator<PostOfficeSender, EmptyInventoriesWorkplaceBehavior>(false)
            .AddDecorator<PostOfficeReceiver, Emptiable>(false)
            .AddDecorator<PostOfficeReceiver, InventoryGoodConsumptionBlocker>(false)
            .AddDecorator<PostOfficeReceiver, PostOfficeWorkWorkplaceBehavior>(false)
            .AddDecorator<PostOfficeReceiver, PostOfficeStatus>(false)
            .AddDecorator<PostOfficeReceiver, RemoveUnwantedStockWorkplaceBehavior>(false)
            .AddDecorator<PostOfficeReceiver, EmptyInventoriesWorkplaceBehavior>(false)
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
