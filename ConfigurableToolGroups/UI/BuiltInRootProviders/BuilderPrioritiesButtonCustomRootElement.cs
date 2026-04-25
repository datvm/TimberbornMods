namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class BuilderPrioritiesButtonCustomRootElement(BuilderPrioritiesButton button, GroupedBuiltInButtonCustomRootElementDI di) : GroupedBuiltInButtonCustomRootElement<BuilderPrioritiesButton>(button, di)
{
    protected override string ToolGroupId { get; } = BuilderPrioritiesButton.ToolGroupId;
    protected override int ReservedOrder => 300;

    protected override ToolHotkeyDefinitionBase GetHotkeyDefinition(ToolButton btn)
    {
        var tool = (BuilderPriorityTool)btn.Tool;
        var description = tool.DescribeTool();

        var id = $"Tool.{nameof(BuilderPriorityTool)}.{tool._priority}";
        var loc = GetLocKey(tool);

        return new ButtonToolHotkeyDefinition(id, loc, btn);
    }

    protected override void RegisterToolButton(VisualElement el, ToolButton btn)
    {
        var locKey = GetLocKey((BuilderPriorityTool)btn.Tool);

        RegisterTool(btn, locKey, locKey);
    }

    static string GetLocKey(BuilderPriorityTool tool) => "Priorities." + tool._priority;
}
