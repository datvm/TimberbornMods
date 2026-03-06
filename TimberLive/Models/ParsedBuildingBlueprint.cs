namespace TimberLive.Models;

public class ParsedBuildingBlueprint(HttpBlueprint blueprint, string path) : ParsedTemplateBlueprintBase(blueprint, path)
{
    public ParsedPlaceableBlockObjectSpec PlaceableBlockObjectSpec { get; } = blueprint.GetComponent<ParsedPlaceableBlockObjectSpec>();

    public override TemplateType Type => TemplateType.Building;
    public override int Order => PlaceableBlockObjectSpec.ToolOrder;
}
