namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class DemolishingButtonCustomRootElement(
    DemolishingButton button,
    GroupedBuiltInButtonCustomRootElementDI di
) : GroupedBuiltInButtonCustomRootElement<DemolishingButton>(button, di)
{
    protected override string ToolGroupId { get; } = DemolishingButton.ToolGroupId;
    protected override int ReservedOrder { get; } = 200;

    protected override ToolHotkeyDefinitionBase? GetHotkeyDefinition(ToolButton btn)
    {
        var id = GetLocKey(btn);        
        if (id is null) { return null; }

        return new ButtonToolHotkeyDefinition(id, id, btn)
        {
            IsDevTool = btn.Tool is EntityBlockObjectDeletionTool,
        };
    }

    static string? GetLocKey(ToolButton btn) => btn.Tool switch
    {
        BuildingDeconstructionTool => BuildingDeconstructionTool.ToolTitleLocKey,
        RecoveredGoodStackDeletionTool => RecoveredGoodStackDeletionTool.ToolTitleLocKey,
        DemolishableSelectionTool => DemolishableSelectionTool.TitleLocKey,
        EntityBlockObjectDeletionTool => EntityBlockObjectDeletionTool.ToolTitleLocKey,
        _ => null,
    };

    protected override void RegisterToolButton(VisualElement el, ToolButton btn)
    {
        var id = GetLocKey(btn);
        if (id is null) { return; }

        RegisterTool(btn, id, id);
    }
}
