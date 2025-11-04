namespace ConfigurableTubeZipLine.Services;

public class ZiplineTemplateModifier : ITemplateModifier
{

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        for (int i = 0; i < template.Specs.Count; i++)
        {
            switch (template.Specs[i])
            {
                case ZiplineTowerSpec towerSpec:
                    template.Specs[i] = towerSpec with
                    {
                        MaxConnections = MSettings.ZiplineMaxConnection,
                        MaxDistance = MSettings.ZiplineMaxDistance,
                    };
                    break;
                case MovementSpeedBoostingBuildingSpec speedSpec:
                    template.Specs[i] = speedSpec with
                    {
                        BoostPercentage = MSettings.ZiplineSpeed,
                    };
                    break;
            }
        }

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => originalTemplateSpec.IsZipline();
}
