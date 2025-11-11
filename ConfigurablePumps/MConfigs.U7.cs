using Timberborn.TemplateSystem;
using Timberborn.WaterBuildings;

namespace ConfigurablePumps;

[Context("MainMenu")]
[Context("Game")]
public class SettingsConfiguration : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder builder = new();
            builder.AddDecorator<WaterMover, MechPumpPowerModifier>();

            return builder.Build();
        }).AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment _)
    {
        new Harmony(nameof(ConfigurablePumps)).PatchAll();
    }

}