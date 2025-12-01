
namespace ConfigurableToolGroups;

[Context("Game")]
[Context("MapEditor")]
public class ConfigurableToolGroupsConfig : Configurator
{

    public override void Configure()
    {
        Bind<ModdableToolGroupSpecService>().AsSingleton();
        Bind<ModdableToolGroupButtonService>().AsSingleton();
        Bind<ToolPanelPositioningService>().AsSingleton();
    }
}
