
namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class TreeCuttingAreaButtonCustomRootElement(TreeCuttingAreaButton button, ToolButtonService toolButtonService) 
    : GroupedBuiltInButtonCustomRootElement<TreeCuttingAreaButton>(button, toolButtonService)
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
}
