namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record FloodgateSettingsModel(float Height, bool IsSynchronized);
public class FloodgateSettings(ILoc t) : BuildingSettingsBase<Floodgate, FloodgateSettingsModel>(t)
{
    
    public override string DescribeModel(FloodgateSettingsModel model) => t.T("LV.MT.BldSet.Floodgate.Desc",
        model.Height, t.TYesNo(model.IsSynchronized));

    protected override bool ApplyModel(FloodgateSettingsModel model, Floodgate target)
    {
        target.ToggleSynchronization(model.IsSynchronized);
        target.SetHeightAndSynchronize(Mathf.Min(model.Height, target.MaxHeight));
        return true;
    }

    protected override FloodgateSettingsModel GetModel(Floodgate duplicable) => new(duplicable.Height, duplicable.IsSynchronized);
}