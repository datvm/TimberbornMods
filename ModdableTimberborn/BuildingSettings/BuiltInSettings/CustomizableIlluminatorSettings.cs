namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record CustomizableIlluminatorSettingsModel(bool IsCustomized, SerializableFloats? CustomColor);

public class CustomizableIlluminatorSettings(ILoc t) : BuildingSettingsBase<CustomizableIlluminator, CustomizableIlluminatorSettingsModel>(t)
{
    public override string DescribeModel(CustomizableIlluminatorSettingsModel model)
        => model.IsCustomized && model.CustomColor.HasValue ? ColorUtility.ToHtmlStringRGBA(model.CustomColor.Value) : t.TNone();

    protected override bool ApplyModel(CustomizableIlluminatorSettingsModel model, CustomizableIlluminator target)
    {
        target.IsCustomized = model.IsCustomized;
        if (model.CustomColor.HasValue)
        {
            target._customColor = model.CustomColor.Value;
        }
        target.Apply();
        return true;
    }

    protected override CustomizableIlluminatorSettingsModel GetModel(CustomizableIlluminator target)
        => new(target.IsCustomized, target._customColor);
}