namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class PausableBuildingSettings(ILoc t) : BuildingSettingsBase<PausableBuilding, BoolSettingModel>(t), IBuildingSettings
{
    int IBuildingSettings.Order => 100;

    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, PausableBuilding target)
    {
        if (model)
        {
            target.Pause();
        }
        else
        {
            target.Resume();
        }

        return true;
    }

    protected override BoolSettingModel GetModel(PausableBuilding duplicable) => duplicable.Paused;
}
