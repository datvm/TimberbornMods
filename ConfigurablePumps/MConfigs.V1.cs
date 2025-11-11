
namespace ConfigurablePumps;

public class MConfig : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();
        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModifier<PumpDepthModifier>()
            .BindTemplateModifier<MechPumpModifier>()
            .BindSpecModifier<WaterRecipeModifier>()
        ;
    }
}