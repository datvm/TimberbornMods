namespace ConfigurableToolGroups.Models;

public class ModdableToolGroupButtonInfo(ToolGroupButton button, ModdableToolGroupButtonInfo? parent)
{

    public readonly ToolGroupButton Button = button;
    public readonly List<ToolGroupButton> Children = [];
    public readonly ModdableToolGroupButtonInfo? Parent = parent;

}
