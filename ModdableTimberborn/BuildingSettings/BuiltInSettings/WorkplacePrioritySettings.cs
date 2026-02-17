namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class WorkplacePrioritySettings(ILoc t) : BuildingSettingsBase<WorkplacePriority, PrioritySettingModel>(t)
{
    public override string DescribeModel(PrioritySettingModel model) => model.T(t);

    protected override bool ApplyModel(PrioritySettingModel model, WorkplacePriority target)
    {
        target.SetPriority(model);
        return true;
    }

    protected override PrioritySettingModel GetModel(WorkplacePriority duplicable) => duplicable.Priority;
}