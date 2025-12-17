namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public abstract class PlantingBuiltInButtonCustomRootElement<T>(T button, ToolButtonService toolButtonService) : GroupedBuiltInButtonCustomRootElement<T>(button, toolButtonService)
    where T : IBottomBarElementsProvider
{

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

        var spec = tool.PlantableSpec.GetSpec<LabeledEntitySpec>();
        var id = $"Tool.Planting.{tool.PlantableSpec.TemplateName}";
        var locKey = spec.DisplayNameLocKey;

        return new ButtonToolHotkeyDefinition(id, locKey, btn)
        {
            GroupId = PlantingGroup,
        };
    }

}

public class FieldsButtonCustomRootElement(FieldsButton button, ToolButtonService toolButtonService) : PlantingBuiltInButtonCustomRootElement<FieldsButton>(button, toolButtonService)
{
    protected override string ToolGroupId { get; } = FieldsButton.ToolGroupId;
    protected override int ReservedOrder => 100;

}

public class ForestryButtonCustomRootElement(ForestryButton button, ToolButtonService toolButtonService) : PlantingBuiltInButtonCustomRootElement<ForestryButton>(button, toolButtonService)
{
    protected override string ToolGroupId { get; } = ForestryButton.ToolGroupId;
    protected override int ReservedOrder => 300;
}