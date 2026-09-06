namespace ConfigurablePumps.Services;

[MultiBind(typeof(ITemplateModifier))]
public class PumpDepthModifier : ITemplateModifier
{

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        template.TransformSpec<WaterInputPipeSpec>(src => src with
        {
            MaxDepth = ModifyDepth(src.MaxDepth),
        });

        return template;
    }

    static int ModifyDepth(int original)
    {
        if (MSettings.AllMultiplier)
        {
            return Mathf.FloorToInt(original * MSettings.Multiplier);
        }
        else if (MSettings.AllFixedDepth)
        {
            return MSettings.FixedDepth;
        }

        throw new InvalidOperationException("Neither AllMultiplier nor AllFixedDepth is enabled");
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => (MSettings.AllMultiplier || MSettings.AllFixedDepth) && originalTemplateSpec.HasSpec<WaterInputPipeSpec>();

}
