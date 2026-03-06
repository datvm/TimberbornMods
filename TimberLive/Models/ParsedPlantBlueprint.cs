namespace TimberLive.Models;

public class ParsedPlantBlueprint : ParsedTemplateBlueprintBase
{
    public ParsedNaturalResourceSpec NaturalResourceSpec { get; }
    public ParsedPlantableSpec PlantableSpec { get; }
    public bool IsForesterPlant => PlantableSpec.ResourceGroup == "Forester";

    public override TemplateType Type { get; }
    public override int Order => NaturalResourceSpec.Order;

    public ParsedPlantBlueprint(HttpBlueprint blueprint, string path) : base(blueprint, path)
    {
        NaturalResourceSpec = blueprint.GetComponent<ParsedNaturalResourceSpec>();
        PlantableSpec = blueprint.GetComponent<ParsedPlantableSpec>();

        if (blueprint.HasComponent<ParsedTreeComponentSpec>() || IsForesterPlant)
        {
            Type = TemplateType.Tree;
        }
        else if (blueprint.HasComponent<ParsedCropSpec>())
        {
            Type = TemplateType.Crop;
        }
        else
        {
            throw new Exception($"Blueprint {blueprint.Name} has PlantableSpec but is neither a tree nor a crop");
        }
    }

}
