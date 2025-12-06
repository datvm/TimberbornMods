namespace ConfigurableExplosives.Services;

public class DynamiteTemplateModifier : ITemplateModifier
{
    public const string ExtractId = "Extract";

    const int BaseScienceCost = 600;
    const int StepScienceCost = 300;

    const int StepExtractCost = 1;

    static readonly ImmutableArray<string> DynamiteTemplatePrefix = [
        "Dynamite.",
        "DoubleDynamite.",
        "TripleDynamite."
    ];

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var depth = GetPrefabDepth(originalTemplateSpec.TemplateName);
        if (depth == -1) { return null; } // Should not happen

        var maxDepth = MSettings.MaxDepths[depth];

        template.TransformSpecs(spec => spec switch
        {
            DynamiteSpec d => d with
            {
                Depth = maxDepth,
            },
            BuildingSpec b => b with
            {
                BuildingCost = SetExtractCost(b.BuildingCost, maxDepth),
                ScienceCost = CalculateScienceCost(maxDepth),
            },
            _ => null,
        });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => DynamiteTemplatePrefix.FastAny(templateName.StartsWith);

    static int GetPrefabDepth(string name)
    {
        int depth = -1;
        for (int i = 0; i < DynamiteTemplatePrefix.Length; i++)
        {
            if (!name.StartsWith(DynamiteTemplatePrefix[i])) { continue; }

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
