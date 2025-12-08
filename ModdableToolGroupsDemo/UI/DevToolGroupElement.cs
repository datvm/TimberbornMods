namespace ModdableToolGroupsDemo.UI;

public class DevToolGroupElement(
    ToolGroupButtonFactory toolGroupButtonFactory,
    ToolGroupService toolGroupService,
    ToolButtonFactory toolButtonFactory,
    BotGeneratorTool botGeneratorTool,
    BeaverGeneratorTool beaverGeneratorTool,
    WaterHeightBrushTool waterHeightBrushTool
) : CustomToolGroupElement(toolGroupButtonFactory, toolGroupService, toolButtonFactory)
{
    public override string Id { get; } = "LV.DevTools";

    protected override void AddChildren(ToolGroupButton parentButton) => 
        AddToolButtonChild(parentButton, 
        [
            (botGeneratorTool, BotGeneratorButton.ToolImageKey),
            (beaverGeneratorTool, BeaverGeneratorButton.ToolImageKey),
            (waterHeightBrushTool, WaterHeightBrushButton.ToolImageKey),
        ]);

}
