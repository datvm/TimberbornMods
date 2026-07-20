namespace TechTree.Services;

[BindSingleton]
public class TechTreeRegistry(
    ISpecService specs,
    ILoc t
) : ILoadableSingleton
{
    public const string DefaultCategoryId = "Default";

    public FrozenDictionary<string, TechItem> TechByIds { get; private set; } = null!;
    public FrozenDictionary<string, TechCategory> CategoryByIds { get; private set; } = null!;
    public TechCategory DefaultCategory { get; private set; } = null!;

    public ImmutableArray<TechCategory> Categories { get; private set; }

    public void Load()
    {
        LoadCategories();
        LoadTechs();
    }

    void LoadCategories()
    {
        Dictionary<string, TechCategory> cats = [];

        foreach (var spec in specs.GetSpecs<TechTreeCategorySpec>())
        {
            if (cats.ContainsKey(spec.Id))
            {
                throw new Exception($"Duplicate TechCategorySpec Id: {spec.Id}, from {spec.Blueprint.Name}");
            }

            var cat = new TechCategory(spec);
            cats[spec.Id] = cat;
            if (cat.Id == DefaultCategoryId)
            {
                DefaultCategory = cat;
            }
        }

        if (DefaultCategory is null)
        {
            throw new Exception($"Missing default TechCategorySpec with Id: {DefaultCategoryId}");
        }

        Categories = [.. cats.Values.OrderBy(c => c.Spec.Order)];
        CategoryByIds = cats.ToFrozenDictionary();
    }

    void LoadTechs()
    {
        Dictionary<string, TechItem> techs = [];
        Dictionary<TechCategory, List<TechItem>> techsByCategory = [];
        var cats = CategoryByIds;

        foreach (var spec in GetBuildingTechs().Concat(GetStandaloneTechs()))
        {
            if (techs.ContainsKey(spec.Id))
            {
                throw new Exception($"Duplicate TechTreeItemSpec Id: {spec.Id}, from {spec.Blueprint.Name}");
            }

            var cat = spec.CategoryId is null 
                ? DefaultCategory 
                : cats.GetValueOrDefault(spec.CategoryId) 
                ?? throw new Exception($"TechTreeItemSpec {spec.Id} has unknown CategoryId: {spec.CategoryId}");

            var tech = new TechItem(spec, cat);
            techs[tech.Id] = tech;
            techsByCategory.GetOrAdd(cat, () => []).Add(tech);
        }

        foreach (var (tc, t) in techsByCategory)
        {
            tc.Techs = [.. t.OrderBy(t => t.Spec.Order).ThenBy(t => t.Id)];
        }

        TechByIds = techs.ToFrozenDictionary();
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
        foreach (var bldSpec in specs.GetSpecs<BuildingSpec>())
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

            if (string.IsNullOrEmpty(nameLoc))
            {
                Debug.LogWarning($"Empty nameLoc for building {templateName}");
            }

            LocalizedText? desc = descLoc is null ? null : new(t.T(descLoc));

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
                    Cost = cost,
                };
            }
            else
            {
                yield return techSpec with
                {
                    NameLoc = nameLoc,
                    Name = new(t.T(nameLoc)),
                    DescriptionLoc = descLoc,
                    Description = desc,
                    Icon = icon,
                    Cost = cost,
                };
            }
        }
    }

    static string? NullIfNullOrEmpty(string? input) => string.IsNullOrEmpty(input) ? null : input;

}
