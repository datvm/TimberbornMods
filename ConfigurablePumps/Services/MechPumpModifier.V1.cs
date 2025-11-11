namespace ConfigurablePumps.Services;

public class MechPumpModifier : ITemplateModifier
{
    bool changeWater = MSettings.MechPumpWaterMul != 1f;
    bool shouldRun = MSettings.MechPumpWaterMul != 1f || MSettings.MechPumpPowerMultiplier != 1f;
    float powerRatio = MSettings.MechPumpWaterMul * MSettings.MechPumpPowerMultiplier;

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        if (changeWater)
        {
            template.TransformSpec<WaterMoverSpec>(src => src with
            {
                WaterPerSecond = src.WaterPerSecond * MSettings.MechPumpWaterMul,
            });
        }
        
        if (changeWater || MSettings.MechPumpPowerMultiplier != 1f)
        {
            template.TransformSpec<MechanicalNodeSpec>(src => src with
            {
                PowerInput = Mathf.RoundToInt(src.PowerInput * powerRatio),
            });
        }

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => shouldRun && originalTemplateSpec.HasSpec<WaterMoverSpec>();
}
