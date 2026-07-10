namespace ConveyorBelt.Services;

[MultiBind(typeof(ITemplateModifier))]
public class ConveyorBeltTemplateModifier(MSettings s) : ITemplateModifier
{

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        template.TransformSpec<MechanicalNodeSpec>(mech => mech with { PowerInput = 0, });
        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => s.NoPower.Value
        && originalTemplateSpec.HasSpec<ConveyorBeltModelSpec>()
        && originalTemplateSpec.HasSpec<MechanicalNodeSpec>();
}
