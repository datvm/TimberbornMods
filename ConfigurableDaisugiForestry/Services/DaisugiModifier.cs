namespace ConfigurableDaisugiForestry.Services;

public class DaisugiModifier(MSettings s) : ITemplateModifier
{
    public const string OakTemplateName = "Oak";
    public const string BirchTemplateName = "Birch";

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var (days, logs, harvestTime, isPlank) = originalTemplateSpec.TemplateName switch
        {
            OakTemplateName => s.OakValues,
            BirchTemplateName => s.BirchValues,
            _ => throw new ArgumentOutOfRangeException(),
        };

        template.TransformSpec<GatherableSpec>(spec => spec with
        {
            YieldGrowthTimeInDays = days,
            YielderSpec = spec.YielderSpec with
            {
                Yield = new()
                {
                    Id = isPlank ? "Plank" : "Log",
                    Amount = logs,
                },
                RemovalTimeInHours = harvestTime,
            },
        });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec) 
        => templateName == OakTemplateName || templateName == BirchTemplateName;
}
