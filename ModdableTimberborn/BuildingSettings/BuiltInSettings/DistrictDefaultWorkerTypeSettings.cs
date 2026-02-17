namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class DistrictDefaultWorkerTypeSettings(
    WorkerTypeHelper workerTypeHelper,
    ILoc t
) : BuildingSettingsBase<DistrictDefaultWorkerType, StringSettingModel>(t)
{
    public override string DescribeModel(StringSettingModel model) => workerTypeHelper.GetDisplayText(model);

    protected override bool ApplyModel(StringSettingModel model, DistrictDefaultWorkerType target)
    {
        target.SetWorkerType(model.Value);
        return true;
    }

    protected override StringSettingModel GetModel(DistrictDefaultWorkerType duplicable) => duplicable.WorkerType;
}