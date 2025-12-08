global using ModdableToolGroupsDemo.UI;

namespace ModdableToolGroupsDemo;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            // Remove original buttons
            .MultiBindElementsRemover<OriginalDevButtonsRemover>()
            .MultiBindCustomTool<DevToolGroupElement>()
        ;
    }
}
