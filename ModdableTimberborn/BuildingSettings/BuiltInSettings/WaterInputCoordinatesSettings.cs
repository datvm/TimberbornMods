namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record WaterInputPipeCoordinatesSettingsModel(
    bool UseDepthLimit,
    int DepthLimit
);

public class WaterInputCoordinatesSettings(ILoc t) : BuildingSettingsBase<WaterInputPipeCoordinates, WaterInputPipeCoordinatesSettingsModel>(t)
{
    public override string DescribeModel(WaterInputPipeCoordinatesSettingsModel model)
        => model.UseDepthLimit ? (model.DepthLimit + "m") : t.TNone();

    protected override bool ApplyModel(WaterInputPipeCoordinatesSettingsModel model, WaterInputPipeCoordinates target)
    {
        target.UseDepthLimit = model.UseDepthLimit;
        target.DepthLimit = Math.Min(model.DepthLimit, target._waterInputPipeSpec.MaxDepth);
        target.UpdateCoordinatesAndDepth();

        return true;
    }

    protected override WaterInputPipeCoordinatesSettingsModel GetModel(WaterInputPipeCoordinates duplicable) 
        => new(duplicable.UseDepthLimit, duplicable.DepthLimit);
}