namespace ConfigurableTubeZipLine.Services;

public class TubewayTemplateModifier : ITemplateModifier
{
    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        for (int i = 0; i < template.Specs.Count; i++)
        {
            switch (template.Specs[i])
            {
                case MovementSpeedBoostingBuildingSpec speedSpec:
                    template.Specs[i] = speedSpec with
                    {
                        BoostPercentage = MSettings.ZiplineSpeed,
                    };
                    break;
                case BlockObjectNavMeshSettingsSpec navMeshSpec:
                    template.Specs[i] = navMeshSpec with
                    {
                        EdgeGroups = [..navMeshSpec.EdgeGroups
                            .Select(eg => eg with {
                                Cost = ModHelpers.CalculateCost(MSettings.TubewaySpeed)
                            })
                        ],
                    };
                    break;
            }
        }

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec) 
        => originalTemplateSpec.IsTubeway() && originalTemplateSpec.HasSpec<BlockObjectNavMeshSettingsSpec>();
}
