namespace ConveyorBelt.Services;

[MultiBind(typeof(ITemplateModifier))]
public class ConveyorBeltTemplateModifier(MSettings s) : ITemplateModifier
{

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        if (s.NoPower.Value)
        {
            template.TransformSpec<MechanicalNodeSpec>(mech => mech with { PowerInput = 0, });
        }
        
        if (s.EarlierAvailability.Value)
        {
            template.TransformSpec<BuildingSpec>(b => b with
            {
                ScienceCost = b.ScienceCost / 15,
                BuildingCost = [new() {
                    Id = "Log",
                    Amount = 1,
                }],
            });
        }

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => s.ShouldModifyTemplates
        && originalTemplateSpec.HasSpec<ConveyorBeltModelSpec>();
}
