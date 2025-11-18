namespace ConfigurableExplosives.Services;

public class DynamiteTemplateModifier : ITemplateModifier
{
    public const string ExtractId = "Extract";

    const int BaseScienceCost = 600;
    const int StepScienceCost = 300;

    const int StepExtractCost = 1;

    static readonly ImmutableArray<ImmutableHashSet<string>> DynamitePrefabNames = [
        ["Dynamite.Folktails", "Dynamite.IronTeeth"],
        ["DoubleDynamite.Folktails", "DoubleDynamite.IronTeeth"],
        ["TripleDynamite.Folktails", "TripleDynamite.IronTeeth"]
    ];
    static readonly FrozenSet<string> AllNames = [..DynamitePrefabNames.SelectMany(x => x)];

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var depth = GetPrefabDepth(originalTemplateSpec.TemplateName);
        if (depth == -1) { return null; } // Should not happen

        var maxDepth = MSettings.MaxDepths[depth];

        template.TransformSpecs(spec =>
        {
            switch (spec)
            {
                case DynamiteSpec d:
                    return d with
                    {
                        Depth = maxDepth,
                    };
                case BuildingSpec b:
                    return b with
                    {
                        BuildingCost = SetExtractCost(b.BuildingCost, maxDepth),
                        ScienceCost = CalculateScienceCost(maxDepth),
                    };
            }

            return null;
        });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => AllNames.Contains(templateName);

    static int GetPrefabDepth(string name)
    {
        int depth = -1;
        for (int i = 0; i < DynamitePrefabNames.Length; i++)
        {
            if (!DynamitePrefabNames[i].Contains(name)) { continue; }

            depth = i;
            break;
        }

        return depth;
    }

    static ImmutableArray<GoodAmountSpec> SetExtractCost(ImmutableArray<GoodAmountSpec> goods, int maxDepth)
    {
        var extractCost = CalculateExtractCost(maxDepth);

        var costWithoutExtract = goods.Where(c => c.Id != ExtractId).ToList();

        if (extractCost > 0)
        {
            costWithoutExtract.Add(new()
            {
                Id = ExtractId,
                Amount = extractCost,
            });
        }

        return [.. costWithoutExtract];
    }

    static int CalculateScienceCost(int maxDepth) => BaseScienceCost + (maxDepth - 1) * (MSettings.NoCostIncrease ? 0 : StepScienceCost);

    static int CalculateExtractCost(int maxDepth) => MSettings.NoCostIncrease ? 0 : StepExtractCost * (maxDepth - 1);

}
