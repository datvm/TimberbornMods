namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class WorkplaceWorkerTypeSettings(
    WorkerTypeHelper workerTypeHelper,
    ILoc t
) : BuildingSettingsBase<WorkplaceWorkerType, StringSettingModel>(t)
{
    public override string DescribeModel(StringSettingModel model) => workerTypeHelper.GetDisplayText(model);

    protected override bool ApplyModel(StringSettingModel model, WorkplaceWorkerType target)
    {
        var w = model.Value;
        if (!target.IsWorkerTypeAllowed(w) || !target.IsWorkerTypeUnlocked(w))
        {
            return false;
        }

        target.SetWorkerType(w);
        return true;
    }

    protected override StringSettingModel GetModel(WorkplaceWorkerType duplicable) => duplicable.WorkerType;
}