namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record FloodgateSettingsModel(
    float Height,
    bool IsSynchronized,
    float AutomationHeight = 0
);

public class FloodgateSettings(ILoc t) : BuildingSettingsBase<Floodgate, FloodgateSettingsModel>(t)
{
    
    public override string DescribeModel(FloodgateSettingsModel model) => t.T("LV.MT.BldSet.Floodgate.Desc",
        model.Height, t.TYesNo(model.IsSynchronized));

    protected override bool ApplyModel(FloodgateSettingsModel model, Floodgate target)
    {
        target.IsSynchronized = model.IsSynchronized;
        target.Height = target.ClampHeight(model.Height);
        target.AutomationHeight = target.ClampHeight(model.AutomationHeight);
        target.UpdateEffectiveHeight(false);
        target.SynchronizeAllNeighbors();
        return true;
    }

    protected override FloodgateSettingsModel GetModel(Floodgate duplicable)
        => new(duplicable.Height, duplicable.IsSynchronized, duplicable.AutomationHeight);
}