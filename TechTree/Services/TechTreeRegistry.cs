namespace TechTree.Services;

[BindSingleton]
public class TechTreeRegistry(
    ISpecService specs,
    TemplateService templateService,
    ILoc t
) : ILoadableSingleton
{
    public const string DefaultCategoryId = "Default";

    /// <summary>
    /// Added to each <see cref="BlockObjectToolGroupSpec.Order"/> so custom categories
    /// can still insert before (0–999) or between tool-group bands.
    /// </summary>
    public const int ToolGroupCategoryOrderPadding = 1000;

    public FrozenDictionary<string, TechItem> TechByIds { get; private set; } = null!;
    public FrozenDictionary<string, TechCategory> CategoryByIds { get; private set; } = null!;
    public FrozenDictionary<string, FrozenSet<TechItem>> TechsByTags { get; private set; } = null!;
    public TechCategory DefaultCategory { get; private set; } = null!;

    public ImmutableArray<TechCategory> Categories { get; private set; }

    public void Load()
    {
        LoadCategories();
        LoadTechs();
        ValidateTechs();
    }

    void LoadCategories()
    {
        Dictionary<string, TechCategory> cats = [];

        // Auto categories from building tool groups (like GetBuildingTechs).
        foreach (var spec in GetToolGroupCategories())
        {
            cats[spec.Id] = new TechCategory(spec);
        }

        // Explicit blueprints add or override (e.g. Default, custom branches).
        foreach (var spec in specs.GetSpecs<TechTreeCategorySpec>())
        {
            var cat = new TechCategory(spec);
            cats[spec.Id] = cat;
        }

        if (!cats.TryGetValue(DefaultCategoryId, out var defaultCategory))
        {
            throw new Exception($"Missing default TechCategorySpec with Id: {DefaultCategoryId}");
        }

        DefaultCategory = defaultCategory;
        Categories = [.. cats.Values.OrderBy(c => c.Spec.Order).ThenBy(c => c.Id, StringComparer.Ordinal)];
        CategoryByIds = cats.ToFrozenDictionary();
    }

    IEnumerable<TechTreeCategorySpec> GetToolGroupCategories()
    {
        foreach (var group in specs.GetSpecs<BlockObjectToolGroupSpec>())
        {
            yield return new()
            {
                Id = group.Id,
                Order = group.Order + ToolGroupCategoryOrderPadding,
                NameLoc = group.NameLocKey,
                Name = new(t.T(group.NameLocKey)),
                Icon = group.Icon,
            };
        }
    }

    void LoadTechs()
    {
        Dictionary<string, TechItem> techs = [];
        Dictionary<TechCategory, List<TechItem>> techsByCategory = [];
        Dictionary<string, List<TechItem>> techsByTag = [];
        var cats = CategoryByIds;

        foreach (var spec in GetBuildingTechs().Concat(GetStandaloneTechs()))
        {
            if (techs.ContainsKey(spec.Id))
            {
                throw new Exception($"Duplicate TechTreeItemSpec Id: {spec.Id}, from {spec.Blueprint.Name}");
            }

            var cat = ResolveCategory(spec, cats);

            var tech = new TechItem(spec, cat);
            techs[tech.Id] = tech;
            techsByCategory.GetOrAdd(cat, () => []).Add(tech);

            foreach (var tag in spec.Tags)
            {
                techsByTag.GetOrAdd(tag, () => []).Add(tech);
            }
        }

        foreach (var (tc, t) in techsByCategory)
        {
            tc.Techs = [.. t.OrderBy(t => t.Spec.Order).ThenBy(t => t.Id)];
        }

        TechByIds = techs.ToFrozenDictionary();
        TechsByTags = techsByTag.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToFrozenSet());
    }

    void ValidateTechs()
    {
        foreach (var t in TechByIds.Values)
        {
            if (t.Spec.Prerequisites is var preq && preq.Length == 0) { continue; }

            if (t.Category.Id == DefaultCategoryId)
            {
                throw new Exception(
                    $"Uncategorized tech {t.Id} cannot have prerequisites. " +
                    $"Assign a CategoryId or remove Prerequisites.");
            }

            foreach (var id in preq)
            {
                if (!TechByIds.ContainsKey(id))
                {
                    throw new Exception($"TechTreeItemSpec {t.Id} has unknown prerequisite: {id}");
                }
            }
        }
    }

    IEnumerable<TechTreeItemSpec> GetStandaloneTechs()
    {
        foreach (var spec in specs.GetSpecs<TechTreeItemSpec>())
        {
            if (spec.HasSpec<BuildingSpec>()) { continue; }
            yield return spec;
        }
    }

    IEnumerable<TechTreeItemSpec> GetBuildingTechs()
    {
        foreach (var bldSpec in templateService.GetAll<BuildingSpec>())
        {
            var placeable = bldSpec.GetSpec<PlaceableBlockObjectSpec>();
            if (placeable is null || placeable.DevModeTool) { continue; }

            var label = bldSpec.GetSpec<LabeledEntitySpec>();
            if (label is null) { continue; }

            var techSpec = bldSpec.GetSpec<TechTreeItemSpec>();

            var templateName = bldSpec.GetTemplateName();
            var nameLoc = NullIfNullOrEmpty(techSpec?.NameLoc) ?? label.DisplayNameLocKey;
            var descLoc = NullIfNullOrEmpty(NullIfNullOrEmpty(techSpec?.DescriptionLoc) ?? label.DescriptionLocKey);
            var icon = techSpec?.Icon ?? label.Icon.Asset;
            var cost = bldSpec.ScienceCost;
            // Explicit CategoryId wins; otherwise use the building tool group (e.g. Wood, Food).
            var categoryId = NullIfNullOrEmpty(techSpec?.CategoryId)
                ?? NullIfNullOrEmpty(placeable.ToolGroupId);

            if (string.IsNullOrEmpty(nameLoc))
            {
                Debug.LogWarning($"Empty nameLoc for building {templateName}");
            }

            LocalizedText? desc = descLoc is null ? null : new(t.T(descLoc));

            // Layout Order defaults to toolbar ToolOrder; tech blueprint can override.
            int order = techSpec is null || techSpec.Order == 0
                ? placeable.ToolOrder
                : techSpec.Order;

            if (techSpec is null)
            {
                yield return new()
                {
                    Id = templateName,
                    NameLoc = nameLoc,
                    Name = new(t.T(nameLoc)),
                    DescriptionLoc = descLoc,
                    Description = desc,
                    Icon = icon,
                    CategoryId = categoryId,
                    Cost = cost,
                    Order = order,
                };
            }
            else
            {
                yield return techSpec with
                {
                    Id = templateName,
                    NameLoc = nameLoc,
                    Name = new(t.T(nameLoc)),
                    DescriptionLoc = descLoc,
                    Description = desc,
                    Icon = icon,
                    CategoryId = categoryId,
                    Cost = cost,
                    Order = order,
                };
            }
        }
    }

    TechCategory ResolveCategory(TechTreeItemSpec spec, FrozenDictionary<string, TechCategory> cats)
    {
        var categoryId = NullIfNullOrEmpty(spec.CategoryId);
        if (categoryId is null)
        {
            return DefaultCategory;
        }

        return cats.GetValueOrDefault(categoryId)
            ?? throw new Exception($"TechTreeItemSpec {spec.Id} has unknown CategoryId: {categoryId}");
    }

    static string? NullIfNullOrEmpty(string? input) => string.IsNullOrEmpty(input) ? null : input;

}
