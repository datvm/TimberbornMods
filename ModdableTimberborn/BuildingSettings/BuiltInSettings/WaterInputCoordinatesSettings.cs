namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record WaterInputCoordinatesSettingsModel(
    bool UseDepthLimit,
    int DepthLimit
);

public class WaterInputCoordinatesSettings(ILoc t) : BuildingSettingsBase<WaterInputCoordinates, WaterInputCoordinatesSettingsModel>(t)
{
    public override string DescribeModel(WaterInputCoordinatesSettingsModel model)
        => model.UseDepthLimit ? (model.DepthLimit + "m") : t.TNone();

    protected override bool ApplyModel(WaterInputCoordinatesSettingsModel model, WaterInputCoordinates target)
    {
        target.UseDepthLimit = model.UseDepthLimit;
        target.DepthLimit = Math.Min(model.DepthLimit, target._waterInputSpec.MaxDepth);
        target.UpdateCoordinatesAndDepth();

        return true;
    }

    protected override WaterInputCoordinatesSettingsModel GetModel(WaterInputCoordinates duplicable) 
        => new(duplicable.UseDepthLimit, duplicable.DepthLimit);
}