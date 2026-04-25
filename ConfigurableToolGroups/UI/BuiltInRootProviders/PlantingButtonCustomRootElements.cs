namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public abstract class PlantingBuiltInButtonCustomRootElement<T>(T button, GroupedBuiltInButtonCustomRootElementDI di) : GroupedBuiltInButtonCustomRootElement<T>(button, di)
    where T : IBottomBarElementsProvider
{

    static string GetLocKey(PlantingTool tool) => tool.PlantableSpec.GetSpec<LabeledEntitySpec>().DisplayNameLocKey;

    protected override void RegisterToolButton(VisualElement el, ToolButton btn)
    {
        var loc = btn.Tool switch
        {
            CancelPlantingTool => CancelPlantingTool.TitleLocKey,
            PlantingTool plantingTool => GetLocKey(plantingTool),
            _ => throw new ArgumentException($"Unexpected tool type {btn.Tool.GetType()}"),
        };

        RegisterTool(btn, loc, loc);
    }

    protected override ToolHotkeyDefinitionBase? GetHotkeyDefinition(ToolButton btn)
    {
        if (btn.Tool is CancelPlantingTool)
        {
            return new ButtonToolHotkeyDefinition(
                $"Tool.{nameof(CancelPlantingTool)}",
                CancelPlantingTool.TitleLocKey,
                btn)
            {
                GroupId = PlantingGroup,
                Order = BuiltInReservedOrder,
            };
        }

        var tool = (PlantingTool)btn.Tool;

        var id = $"Tool.Planting.{tool.PlantableSpec.TemplateName}";
        var locKey = GetLocKey(tool);

        return new ButtonToolHotkeyDefinition(id, locKey, btn)
        {
            GroupId = PlantingGroup,
        };
    }

}

public class FieldsButtonCustomRootElement(FieldsButton button, GroupedBuiltInButtonCustomRootElementDI di) : PlantingBuiltInButtonCustomRootElement<FieldsButton>(button, di)
{
    protected override string ToolGroupId { get; } = FieldsButton.ToolGroupId;
    protected override int ReservedOrder => 100;

}

public class ForestryButtonCustomRootElement(ForestryButton button, GroupedBuiltInButtonCustomRootElementDI di) : PlantingBuiltInButtonCustomRootElement<ForestryButton>(button, di)
{
    protected override string ToolGroupId { get; } = ForestryButton.ToolGroupId;
    protected override int ReservedOrder => 300;
}