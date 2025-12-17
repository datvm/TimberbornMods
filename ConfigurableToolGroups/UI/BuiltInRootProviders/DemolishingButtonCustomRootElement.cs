namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class DemolishingButtonCustomRootElement(DemolishingButton button, ToolButtonService toolButtonService) : GroupedBuiltInButtonCustomRootElement<DemolishingButton>(button, toolButtonService)
{
    protected override string ToolGroupId { get; } = DemolishingButton.ToolGroupId;
    protected override int ReservedOrder { get; } = 200;

    protected override ToolHotkeyDefinitionBase? GetHotkeyDefinition(ToolButton btn)
    {
        string? id = null;
        var isDevTool = false;
        switch (btn.Tool)
        {
            case BuildingDeconstructionTool:
                id = BuildingDeconstructionTool.ToolTitleLocKey;
                break;
            case RecoveredGoodStackDeletionTool:
                id = RecoveredGoodStackDeletionTool.ToolTitleLocKey;
                break;
            case DemolishableSelectionTool:
                id = DemolishableSelectionTool.TitleLocKey;
                break;
            case EntityBlockObjectDeletionTool:
                id = EntityBlockObjectDeletionTool.ToolTitleLocKey;
                isDevTool = true;
                break;
        }
        if (id is null) { return null; }

        return new ButtonToolHotkeyDefinition(id, id, btn)
        {
            IsDevTool = isDevTool,
        };
    }
}
