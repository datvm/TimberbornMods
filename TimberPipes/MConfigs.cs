namespace TimberPipes;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game | ConfigurationContext.MainMenu;

    protected override void ConfigureRegistry(ModdableTimberbornRegistry registry)
        => registry.UseBuildingSettings();

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        if (context != ConfigurationContext.Game) { return; }

        configurator.MultiBind<EntityPanelModule>().ToProvider<DebugFragmentProvider>().AsSingleton();

        configurator.BindTemplateModule(h => h
            .AddDecorator<DischargePipeParticleController, ParticlesCache>(addTransient: false)
        );
    }

    class DebugFragmentProvider(TransportPipeDebugFragment fragment) : IProvider<EntityPanelModule>
    {
        public EntityPanelModule Get()
        {
            var b = new EntityPanelModule.Builder();
            b.AddDiagnosticFragment(fragment);
            return b.Build();
        }
    }
}
