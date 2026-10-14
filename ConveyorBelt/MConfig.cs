namespace ConveyorBelt;

public class MConfig : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);
        if (context != ConfigurationContext.Game)
        {
            return;
        }

        configurator.MultiBind<EntityPanelModule>().ToProvider<DebugFragmentProvider>().AsSingleton();
    }

    class DebugFragmentProvider(BeltDebugFragment fragment) : IProvider<EntityPanelModule>
    {
        public EntityPanelModule Get()
        {
            var b = new EntityPanelModule.Builder();
            b.AddDiagnosticFragment(fragment);
            return b.Build();
        }
    }
}
