namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class BlueprintApiService(ApiService api)
{
    public const string CommonCollectionId = "Common";

    public async Task<FactionTemplateCompilation> GetFactionTemplatesAsync()
    {
        // 1 get all factions and template collections
        var factions = (await GetSpecsAsync<ParsedFactionSpec>())
            .OrderBy(f => f.Order)
            .ToArray();

        // 1.1 Get all the collection ids we need
        var requiredCollectionIds = factions
            .SelectMany(f => f.TemplateCollectionIds)
            .Append(CommonCollectionId)
            .ToHashSet();

        // 2. Get the relevant template collections
        var templateCollections = await GetSpecsAsync<ParsedTemplateCollectionSpec>();
        var neededCollections = templateCollections
            .Where(c => requiredCollectionIds.Contains(c.CollectionId))
            .GroupBy(c => c.CollectionId)
            .Select(grp => (grp.Key, grp.SelectMany(g => g.Blueprints).Distinct().ToArray()))
            .ToDictionary(); // collectionId -> blueprint paths

        // 3. Gather the paths and get the blueprints
        var blueprintsByPaths = await GetAndParseBlueprintsAsync([.. neededCollections.SelectMany(c => c.Value)]);

        // 4. Gather paths for each factions, distinct paths from multiple collections
        Dictionary<string, FactionWithTemplates> factionsWithTemplates = [];
        foreach (var f in factions)
        {
            HashSet<string> paths = [];
            foreach (var colId in f.TemplateCollectionIds.Append(CommonCollectionId))
            {
                if (neededCollections.TryGetValue(colId, out var colPaths))
                {
                    paths.UnionWith(colPaths);
                }
            }

            var templates = new TemplateBlueprintCollection();
            templates.AddRange(paths.Select(p => blueprintsByPaths[p]));

            factionsWithTemplates.Add(f.Id, new(f, templates));
        }

        // 5. Get the tool groups
        var toolGroups = (await GetSpecsAsync<ParsedBlockObjectToolGroupSpec>())
            .OrderBy(q => q.Order)
            .ToArray();

        // 6. Group everythings
        Dictionary<string, FactionWithGroupedTemplates> factionsWithGroupedTemplates = [];
        foreach (var f in factions)
        {
            var fId = f.Id;
            var templates = factionsWithTemplates[fId].Blueprints;

            Dictionary<string, List<IParsedLabeledTemplateBlueprint>> groupedTemplates = [];
            foreach (var b in templates.Buildings)
            {
                var grp = b.PlaceableBlockObjectSpec.ToolGroupId;
                var list = groupedTemplates.GetOrAdd(grp, _ => []);
                list.Add(b);
            }

            factionsWithGroupedTemplates.Add(f.Id, new(
                f,
                new(TemplateType.Tree, null, [.. templates.Trees.OrderBy(t => t.Order)]),
                new(TemplateType.Crop, null, [.. templates.Crops.OrderBy(t => t.Order)]),
                [..toolGroups
                    .Where(tg => groupedTemplates.ContainsKey(tg.Id))
                    .Select(tg => new GroupedTemplate(
                        TemplateType.Building,
                        tg,
                        [.. groupedTemplates[tg.Id].OrderBy(b => b.Order)]))
                ]
            ));
        }

        return new(
            [.. factions],
            [.. toolGroups],
            factionsWithTemplates.ToFrozenDictionary(),
            factionsWithGroupedTemplates.ToFrozenDictionary()
        );
    }

    public static IParsedTemplateBlueprint ParseTemplateBlueprint(HttpBlueprint blueprint, string path)
    {
        if (blueprint.HasComponent<ParsedPlaceableBlockObjectSpec>())
        {
            return new ParsedBuildingBlueprint(blueprint, path);
        }
        else if (blueprint.HasComponent<ParsedNaturalResourceSpec>())
        {
            return new ParsedPlantBlueprint(blueprint, path);
        }
        else
        {
            return new ParsedUnknownBlueprint(blueprint, path);
        }
    }

    async Task<Dictionary<string, IParsedTemplateBlueprint>> GetAndParseBlueprintsAsync(HashSet<string> paths)
    {
        string[] indexed = [.. paths];
        var blueprints = await GetBlueprintsAsync(indexed);

        Dictionary<string, IParsedTemplateBlueprint> result = [];
        for (int i = 0; i < indexed.Length; i++)
        {
            var path = indexed[i];
            var bp = blueprints[i];
            result.Add(path, ParseTemplateBlueprint(bp, path));
        }

        return result;
    }

    public async Task<HttpBlueprint> GetBlueprintAsync(string path) 
        => await api.GetAsync<HttpBlueprint>("blueprints/get?path=" + Uri.EscapeDataString(path));

    public async Task<HttpBlueprint[]> GetBlueprintsAsync(string[] paths)
    {
        var list = string.Join(";", paths);

        using var req = new HttpRequestMessage(HttpMethod.Post, "blueprints/get-many");
        req.Content = new StringContent(list);

        return await api.SendAsync<HttpBlueprint[]>(req);
    }

    public async Task<HttpBlueprint[]> GetBlueprintWithSpecAsync(string specTypeName)
        => await api.GetAsync<HttpBlueprint[]>("blueprints/specs?type=" + Uri.EscapeDataString(specTypeName));

    public async Task<List<T>> GetSpecsAsync<T>()
        where T : ParsedComponentSpec
    {
        var typeName = BlueprintExtensions.GetTypeName<T>();
        var blueprints = await GetBlueprintWithSpecAsync(typeName);

        List<T> result = [];
        foreach (var bp in blueprints)
        {
            result.Add(bp.GetComponent<T>(typeName));
        }

        return result;
    }

    public async Task<Dictionary<string, T>> GetSpecsAsync<T>(Func<T, string> keyFunc)
        where T : ParsedComponentSpec 
        => (await GetSpecsAsync<T>()).ToDictionary(keyFunc);


}
