namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class FarmHouseSettings(ILoc t) : BuildingSettingsBase<FarmHouse, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, FarmHouse target)
    {
        target.PlantingPrioritized = model;
        return true;
    }

    protected override BoolSettingModel GetModel(FarmHouse duplicable) => duplicable.PlantingPrioritized;
}