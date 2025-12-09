namespace ModdableToolGroupsDemo.UI;

public class DevToolGroupElement(
    BotGeneratorTool botGeneratorTool,
    BeaverGeneratorTool beaverGeneratorTool,
    WaterHeightBrushTool waterHeightBrushTool,

    ToolGroupService toolGroupService,
    ModdableToolGroupButtonFactory buttonFac
) : CustomRootToolGroupElement(toolGroupService, buttonFac)
{
    public override string Id { get; } = "LV.DevTools";

    protected override void AddChildren(ModdableToolGroupButton btn)
    {
        btn.AddChildTool(botGeneratorTool, BotGeneratorButton.ToolImageKey);
        btn.AddChildTool(beaverGeneratorTool, BeaverGeneratorButton.ToolImageKey);
        btn.AddChildTool(waterHeightBrushTool, WaterHeightBrushButton.ToolImageKey);
    }
}
