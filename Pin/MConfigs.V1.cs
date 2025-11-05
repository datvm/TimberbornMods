namespace Pin;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<PinService>()

            .BindFragment<PinFragment>()
            .BindTransient<PinPanel>()
            .BindSingleton<PinUnderlay>()

            .BindTransient<PinBatchControlRow>()
            .BindSingleton<PinBatchControlTab>()

            .BindTemplateModule(h => h
                .AddDecorator<PinSpec, PinComponent>()
            )
        ;

        MultiBind<BatchControlModule>().ToProvider<PinBatchControlModuleProvider>().AsSingleton();
    }

    class PinBatchControlModuleProvider(PinBatchControlTab tab) : IProvider<BatchControlModule>
    {
        public BatchControlModule Get()
        {
            BatchControlModule.Builder builder = new();
            builder.AddTab(tab, 400);
            return builder.Build();
        }
    }
}