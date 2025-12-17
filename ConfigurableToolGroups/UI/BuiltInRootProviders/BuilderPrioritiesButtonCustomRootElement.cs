namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class BuilderPrioritiesButtonCustomRootElement(BuilderPrioritiesButton button, ToolButtonService toolButtonService) : GroupedBuiltInButtonCustomRootElement<BuilderPrioritiesButton>(button, toolButtonService)
{
    protected override string ToolGroupId { get; } = BuilderPrioritiesButton.ToolGroupId;
    protected override int ReservedOrder => 300;

    protected override ToolHotkeyDefinitionBase GetHotkeyDefinition(ToolButton btn)
    {
        var tool = (BuilderPriorityTool)btn.Tool;
        var description = tool.DescribeTool();

        var id = $"Tool.{nameof(BuilderPriorityTool)}.{tool._priority}";
        var loc = "Priorities." + tool._priority;

        return new ButtonToolHotkeyDefinition(id, loc, btn);
    }
}
