namespace ConfigurableToolGroups.Models;

public class ToolGroupButtonInfo(ToolGroupButton button, ToolGroupButtonInfo? parent)
{

    public readonly ToolGroupButton Button = button;
    public readonly List<ToolGroupButton> Children = [];
    public readonly ToolGroupButtonInfo? Parent = parent;

}
