namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class WorkplaceSettings(ILoc t) : BuildingSettingsBase<Workplace, IntSettingModel>(t)
{
    public override string DescribeModel(IntSettingModel model) => model.Value.ToString();

    protected override bool ApplyModel(IntSettingModel model, Workplace target)
    {
        target.SetDesiredWorkers(Math.Min(model.Value, target.MaxWorkers));
        return true;
    }

    protected override IntSettingModel GetModel(Workplace duplicable) => duplicable.DesiredWorkers;
}