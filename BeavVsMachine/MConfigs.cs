namespace BeavVsMachine;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseBonusTracker()
            .UseSoakEffect()
            .AddConfigurator<MConfig>();
    }

}

public class MConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context.IsMenuContext())
        {
            configurator.MultiBindSingleton<IModUpdateNotifier, ModUpdateNotifier>();
        }

        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<BeaverExpStatTracker>()
            .BindSingleton<BotPerformanceUpdater>()

            .MultiBindSingleton<IDevModule, BvmDevModule>()

            .BindTemplateModule(h => h
                .AddDecorator<BeaverSpec, BeaverExpComponent>()
                .AddDecorator<BeaverSpec, BeaverFitnessComponent>()
                .AddDecorator<BotSpec, BotAgingPerformanceComponent>()
                .AddDecorator<BotSpec, BotWaterDamageComponent>()
            )
        ;
    }
}

