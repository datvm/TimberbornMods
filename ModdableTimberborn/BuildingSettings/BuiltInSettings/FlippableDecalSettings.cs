namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class FlippableDecalSettings(ILoc t) : BuildingSettingsBase<FlippableDecal, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, FlippableDecal target)
    {
        target.SetFlip(model);
        return true;
    }

    protected override BoolSettingModel GetModel(FlippableDecal duplicable) => duplicable.IsFlipped;
}