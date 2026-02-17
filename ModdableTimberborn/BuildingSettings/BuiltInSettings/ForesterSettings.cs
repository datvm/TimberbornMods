namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class ForesterSettings(ILoc t) : BuildingSettingsBase<Forester, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, Forester target)
    {
        target.ReplantDeadTrees = model;
        return true;
    }

    protected override BoolSettingModel GetModel(Forester duplicable) => duplicable.ReplantDeadTrees;
}