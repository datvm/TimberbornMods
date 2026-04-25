
namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class TreeCuttingAreaButtonCustomRootElement(TreeCuttingAreaButton button, GroupedBuiltInButtonCustomRootElementDI di) 
    : GroupedBuiltInButtonCustomRootElement<TreeCuttingAreaButton>(button, di)
{
    protected override string ToolGroupId { get; } = TreeCuttingAreaButton.ToolGroupId;
    protected override int ReservedOrder => 0;

    protected override ToolHotkeyDefinitionBase? GetHotkeyDefinition(ToolButton btn) => btn.Tool switch
    {
        TreeCuttingAreaSelectionTool => new ButtonToolHotkeyDefinition(
            $"Tool.{nameof(TreeCuttingAreaSelectionTool)}",
            TreeCuttingAreaSelectionTool.TitleLocKey,
            btn),
        TreeCuttingAreaUnselectionTool => new ButtonToolHotkeyDefinition(
            $"Tool.{nameof(TreeCuttingAreaUnselectionTool)}",
            TreeCuttingAreaUnselectionTool.TitleLocKey,
            btn),
        _ => null,
    };

    static string GetLocKey(ToolButton btn) => btn.Tool switch
    {
        TreeCuttingAreaSelectionTool => TreeCuttingAreaSelectionTool.TitleLocKey,
        TreeCuttingAreaUnselectionTool => TreeCuttingAreaUnselectionTool.TitleLocKey,
        _ => throw new ArgumentException($"Unexpected tool type {btn.Tool.GetType()}"),
    };

    protected override void RegisterToolButton(VisualElement el, ToolButton btn)
    {
        var loc = GetLocKey(btn);
        RegisterTool(btn, loc, loc);
    }
}
