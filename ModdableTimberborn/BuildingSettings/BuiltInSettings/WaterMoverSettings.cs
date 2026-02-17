namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record WaterMoverSettingsModel(bool Clean, bool Contaminated);

public class WaterMoverSettings(
    IGoodService goods,
    ILoc t
) : BuildingSettingsBase<WaterMover, WaterMoverSettingsModel>(t), ILoadableSingleton
{

    string cleanWater = "", contaminatedWater = "";

    void ILoadableSingleton.Load()
    {
        base.Load();

        cleanWater = goods.GetGood(WaterMoverToggle.CleanWaterGoodId).DisplayName.Value;
        contaminatedWater = goods.GetGood(WaterMoverToggle.ContaminatedWaterGoodId).DisplayName.Value;
    }

    public override string DescribeModel(WaterMoverSettingsModel model)
    {
        return 
            model.Clean && model.Contaminated ? t.T("WaterMover.Unfiltered")
            : model.Clean                     ? cleanWater
                                              : contaminatedWater;
    }

    protected override bool ApplyModel(WaterMoverSettingsModel model, WaterMover target)
    {
        target.CleanWaterMovement = model.Clean;
        target.ContaminatedWaterMovement = model.Contaminated;

        return true;
    }

    protected override WaterMoverSettingsModel GetModel(WaterMover duplicable)
        => new(duplicable.CleanWaterMovement, duplicable.ContaminatedWaterMovement);
}