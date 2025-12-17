namespace ConfigurableToolGroups.Models;

public abstract class ToolHotkeyDefinitionBase(string id, string locKey) : IToolHotkeyDefinition
{
    public string Id { get; } = id;
    public string LocKey { get; } = locKey;
    public abstract void Select();
    public string GroupId { get; set; } = IToolHotkeyDefinition.DefaultGroupId;
    public int? Order { get; set; }
    public bool IsDevTool { get; set; }
}

public class ButtonToolHotkeyDefinition(string id, string locKey, IToolbarButton button) : ToolHotkeyDefinitionBase(id, locKey)
{
    public override void Select() => button.Select();
}

public class ActionHotkeyDefinition(string id, string locKey, Action action) : ToolHotkeyDefinitionBase(id, locKey)
{
    public override void Select() => action();
}