namespace ModdableToolGroupsDemo.UI;

public class DevToggleButtonElement : CustomRootToolElement<DevToolToggleTool>
{

    public override string Id { get; } = "LV.DevToggleButton";
    protected override string ImageName { get; } = "ToggleDevTool";

    public DevToggleButtonElement(ToolButtonFactory toolButtonFactory, DevToolToggleTool tool) : base(toolButtonFactory, tool)
    {
        Color = RootToolButtonColor.Red;
    }
}
