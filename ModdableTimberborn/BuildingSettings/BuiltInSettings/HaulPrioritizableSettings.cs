namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class HaulPrioritizableSettings(ILoc t) : BuildingSettingsBase<HaulPrioritizable, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, HaulPrioritizable target)
    {
        target.Prioritized = model;
        return true;
    }

    protected override BoolSettingModel GetModel(HaulPrioritizable duplicable) => duplicable.Prioritized;
}