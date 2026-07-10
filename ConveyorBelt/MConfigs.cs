namespace ConveyorBelt;

public class MConveyorBeltConfigs : BaseModdableTimberbornAttributeConfiguration, IWithDIConfig
{

    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        if (context != ConfigurationContext.Game) { return; }

        configurator.MultiBind<EntityPanelModule>().ToProvider<DebugFragmentProvider>().AsSingleton();
    }

    class DebugFragmentProvider(ConveyorBeltDebugFragment f) : IProvider<EntityPanelModule>
    {
        public EntityPanelModule Get()
        {
            var b = new EntityPanelModule.Builder();
            b.AddDiagnosticFragment(f);
            return b.Build();
        }
    }

}
