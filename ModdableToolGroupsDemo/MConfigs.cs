global using ModdableToolGroupsDemo.UI;
global using ModdableToolGroupsDemo.Services;
global using ConfigurableToolGroups.UI.BuiltInRootProviders;

namespace ModdableToolGroupsDemo;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        // Remove original buttons & add the new Dev group
        this
            .MultiBindElementsRemover<OriginalDevButtonsRemover>()
            .MultiBindCustomTool<DevToolGroupElement>()
        ;

        // Add Toggle button
        Bind<DevToolToggleTool>().AsSingleton();
        this.MultiBindCustomTool<DevToggleButtonElement>();

        // Remove the original buttons & add new planting groups
        this
            .MultiBindElementsRemover<OriginalPlantingButtonsRemover>()
            .MultiBindCustomTool<PlantingGroupElement>()
        ;
    }
}
