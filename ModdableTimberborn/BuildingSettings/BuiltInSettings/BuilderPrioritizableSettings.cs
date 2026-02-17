namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class BuilderPrioritizableSettings(ILoc t) : BuildingSettingsBase<BuilderPrioritizable, PrioritySettingModel>(t)
{
    public override string DescribeModel(PrioritySettingModel model) => model.T(t);

    protected override bool ApplyModel(PrioritySettingModel model, BuilderPrioritizable target)
    {
        if (!target.Enabled && !target._blockObject.IsFinished) { return false; }

        target.SetPriority(model);
        return true;
    }

    protected override PrioritySettingModel GetModel(BuilderPrioritizable duplicable) => duplicable.Priority;
}
