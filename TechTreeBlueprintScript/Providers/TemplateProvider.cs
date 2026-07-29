namespace TechTreeBlueprintScript.Providers;

[BindSingleton]
public class TemplateProvider
{
    readonly BlueprintProvider blueprintProvider;

    public FrozenDictionary<string, ImmutableArray<ScriptBlueprint>> TemplatesByFaction { get; }
    public FrozenDictionary<string, ScriptBlueprint> BlueprintByTemplateName { get; }

    public TemplateProvider(BlueprintProvider blueprintProvider)
    {
        this.blueprintProvider = blueprintProvider;

        var factions = blueprintProvider.GetSpecs<FactionSpec>();
        var templateCollections = blueprintProvider.AggregatedCollections[typeof(TemplateCollectionSpec)];

        Dictionary<string, ImmutableArray<ScriptBlueprint>> templatesByFaction = [];

        foreach (var f in factions)
        {
            var id = f.Id;
            var colIds = f.TemplateCollectionIds;

            var templatePaths = colIds
                .SelectMany(colId => templateCollections.Collections[colId])
                .Distinct();

            templatesByFaction[id] = [.. templatePaths.Select(GetTemplateByPath)];
        }

        TemplatesByFaction = templatesByFaction.ToFrozenDictionary();
        BlueprintByTemplateName = blueprintProvider.BlueprintsBySpecType[typeof(TemplateSpec)]
            .ToFrozenDictionary(b => b.GetTemplateName());
    }

    public ScriptBlueprint GetTemplateByPath(string path) => blueprintProvider.BlueprintByPath[path];
    public ScriptBlueprint GetTemplateByName(string name) => BlueprintByTemplateName[name];

}
