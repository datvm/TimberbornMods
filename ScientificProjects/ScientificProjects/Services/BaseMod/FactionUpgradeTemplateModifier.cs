namespace ScientificProjects.Services.BaseMod;

public class FactionUpgradeTemplateModifier : ITemplateModifier
{
    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        template.Specs.Add(new SPFactionUpgradeDescriberSpec());
        template.Specs.RemoveAll(spec => spec is MechanicalNodeSpec || spec is MechanicalBuildingSpec);

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => ScientificProjectsUtils.HasFtUpgradeEffect(templateName)
        || ScientificProjectsUtils.HasItUpgradeEffect(templateName);
}
