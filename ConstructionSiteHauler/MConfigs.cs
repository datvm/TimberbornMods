namespace ConstructionSiteHauler;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{

    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    protected override void ConfigureRegistry(ModdableTimberbornRegistry registry)
        => registry
            .UseExtraHaulerTargets()
            .UseEntityTracker()
            .TryTrack<ConstructionSiteHaulerComponent>()
        ;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<ConstructionSiteHaulerComponent, FillInputWorkplaceBehavior>(addTransient: false)
            );
    }

}
