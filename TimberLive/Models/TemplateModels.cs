
namespace TimberLive.Models;

public record FactionTemplateCompilation(
    ImmutableArray<ParsedFactionSpec> Factions,
    ImmutableArray<ParsedBlockObjectToolGroupSpec> ToolGroups,
    FrozenDictionary<string, FactionWithTemplates> FactionsWithTemplates,
    FrozenDictionary<string, FactionWithGroupedTemplates> FactionsWithGroupedTemplates
);

public record FactionWithTemplates(
    ParsedFactionSpec Faction,
    TemplateBlueprintCollection Blueprints
);

public record FactionWithGroupedTemplates(
    ParsedFactionSpec Faction,
    GroupedTemplate Trees,
    GroupedTemplate Crops,
    ImmutableArray<GroupedTemplate> Buildings
);

public record GroupedTemplate(TemplateType Type, ParsedBlockObjectToolGroupSpec? Group, ImmutableArray<IParsedLabeledTemplateBlueprint> Blueprints);

public class TemplateBlueprintCollection
{
    public List<ParsedBuildingBlueprint> Buildings { get; } = [];
    public List<ParsedPlantBlueprint> Crops { get; } = [];
    public List<ParsedPlantBlueprint> Trees { get; } = [];
    public List<IParsedTemplateBlueprint> Others { get; } = [];

    public IEnumerable<IParsedTemplateBlueprint> AllBlueprints => Buildings.Cast<IParsedTemplateBlueprint>()
        .Concat(Crops)
        .Concat(Trees)
        .Concat(Others);

    public void Add(IParsedTemplateBlueprint blueprint)
    {
        switch (blueprint)
        {
            case ParsedBuildingBlueprint building:
                Buildings.Add(building);
                break;
            case ParsedPlantBlueprint plant:
                if (plant.Type == TemplateType.Tree)
                {
                    Trees.Add(plant);
                }
                else if (plant.Type == TemplateType.Crop)
                {
                    Crops.Add(plant);
                }
                break;
            default:
                Others.Add(blueprint);
                break;
        }
    }

    public void AddRange(IEnumerable<IParsedTemplateBlueprint> blueprints)
    {
        foreach (var blueprint in blueprints)
        {
            Add(blueprint);
        }
    }

}