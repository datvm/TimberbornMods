namespace MechanizedManufactory;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IWithDIConfig, IHarmonyPatchAll
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        if (context != ConfigurationContext.Game) { return; }

        configurator.BindTemplateModule(h => h
            .AddDecorator<MechanizedManufactoryComponent, NoHaulingPostStatus>(false)
        );
    }

}
