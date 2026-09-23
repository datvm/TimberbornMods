namespace ScienceShop;

public class MConfig : BaseModdableTimberbornAttributeConfiguration, IHarmonyPatchAll
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator.MultiBind<EntityPanelModule>().ToProvider<DebugFragmentProvider>().AsSingleton();
    }

    class DebugFragmentProvider(ScienceShopDebugFragment fragment) : IProvider<EntityPanelModule>
    {
        public EntityPanelModule Get()
        {
            var b = new EntityPanelModule.Builder();
            b.AddDiagnosticFragment(fragment);
            return b.Build();
        }
    }
}
