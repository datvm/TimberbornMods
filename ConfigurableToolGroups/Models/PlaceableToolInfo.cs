namespace ConfigurableToolGroups.Models;

public record PlaceableToolInfo(
    PlaceableBlockObjectSpec Placeable,
    string TemplateName
) : IToolButtonInfo
{
    public string Id => TemplateName;
}